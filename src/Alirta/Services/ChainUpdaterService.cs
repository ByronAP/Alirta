using Alirta.Contracts;
using Alirta.DbContexts;
using Alirta.Helpers;
using Alirta.Models;
using ChiaApi;
using LogParser.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Alirta.Services
{
    internal class ChainUpdaterService : IHostedService
    {
        private readonly IChainConfig _chainConfig;
        private readonly ILogger<ChainUpdaterService> _logger;
        private readonly AppDbContext _appDbContext;
        private readonly Timer _timer;
        private readonly FullNodeApiClient _fullNodeApiClient;
        private readonly FarmerApiClient _farmerApiClient;
        private readonly HarvesterApiClient _harvesterApiClient;
        private readonly WalletApiClient _walletApiClient;

        public ChainUpdaterService(IChainConfig chainConfig, ILogger<ChainUpdaterService> logger, AppDbContext appDbContext)
        {
            _chainConfig = chainConfig;
            _logger = logger;
            _appDbContext = appDbContext;

            _appDbContext.ChainItems.Load();
            _appDbContext.MonitorAddresses.Load();
            _appDbContext.PeerItems.Load();

            _fullNodeApiClient = NodeApis.GetFullNodeApiClient(_chainConfig);
            _farmerApiClient = NodeApis.GetFarmerApiClient(_chainConfig);
            _harvesterApiClient = NodeApis.GetHarvesterApiClient(_chainConfig);
            _walletApiClient = NodeApis.GetWalletApiClient(_chainConfig);

            _timer = new Timer(DoWork, null, Timeout.Infinite, Timeout.Infinite);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var randStartDelay = new Random().Next(1, 30);
            _timer.Change(TimeSpan.FromSeconds(randStartDelay), TimeSpan.FromMinutes(1));

            _logger.LogInformation("Monitoring starting for {ChainName} {DisplayName}.", _chainConfig.ChainName.ToUpper(), _chainConfig.InstanceDisplayName);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            _logger.LogInformation("Monitoring stopped for {ChainName} {DisplayName}.", _chainConfig.ChainName.ToUpper(), _chainConfig.InstanceDisplayName);

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            var dbRecord = await _appDbContext.ChainItems.FindAsync(_chainConfig.Id);
            if (dbRecord == null)
            {
                dbRecord = await InitDbRecordAsync();

                _chainConfig.Id = Convert.ToUInt32(dbRecord.Id);
                _chainConfig.Save();

                await UpdateDbRecordAsync(dbRecord);

                return;
            }


            if (DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(dbRecord.LastSubmittedTimestamp)).AddMinutes(Constants.ServerUpdateIntervalMinutes) < DateTimeOffset.UtcNow)
            {
                await UpdateDbRecordAsync(dbRecord);
                // Submit data to server
                _logger.LogCritical("UPLOAD TO SERVER!");
            }
        }

        private async Task<ChainDbItem> InitDbRecordAsync()
        {
            var newDbRecord = new ChainDbItem();
            newDbRecord.ChainName = _chainConfig.ChainName;

            UpdateDbRecordFromChainConfig(newDbRecord);

            var debugLogFilePath = Path.Combine(FileSystem.GetChainLogsDirectoryPath(_chainConfig.ChainFolder, _chainConfig.Network), "debug.log");
            if (File.Exists(debugLogFilePath))
            {
                var logItems = LogParser.Parser.ParseLines(debugLogFilePath);
                if (logItems != null)
                {
                    try
                    {
                        foreach (var logItem in logItems)
                        {
                            // only take the first one
                            newDbRecord.LastLogTimestamp = Convert.ToUInt64(logItem.ProducedAt.ToUnixTimeMilliseconds());
                            break;
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to get/parse initial log item for {ChainName} {DisplayName}.", _chainConfig.ChainName.ToUpper(), _chainConfig.InstanceDisplayName);
                    }
                }
            }
            else
            {
                _logger.LogWarning("Debug log file for {ChainName} {DisplayName} doesn't exist.", _chainConfig.ChainName.ToUpper(), _chainConfig.InstanceDisplayName);
            }

            _appDbContext.ChainItems.Add(newDbRecord);

            await _appDbContext.SaveChangesAsync();

            return newDbRecord;
        }

        private async Task UpdateDbRecordAsync(ChainDbItem dbRecord)
        {
            await _appDbContext.ChainItems.LoadAsync();

            UpdateDbRecordFromChainConfig(dbRecord);

            // check the apis before we bother trying to work with them
            await UpdateDbRecordApiRespondingsAsync(dbRecord);

            var tasks = new List<Task>();
            tasks.Add(UpdateDbRecordFromBlockchainStateAsync(dbRecord));
            tasks.Add(UpdateDbRecordFromLogsAsync(dbRecord));

            await Task.WhenAll(tasks);

            await _appDbContext.SaveChangesAsync();
        }

        private void UpdateDbRecordFromChainConfig(ChainDbItem dbRecord)
        {
            var chainName = _chainConfig.ChainName.ToLower().Trim();
            if (dbRecord.ChainName != chainName) dbRecord.ChainName = chainName;

            var currencyCode = _chainConfig.CurrencyCode.ToLower().Trim();
            if (dbRecord.CurrencyCode != currencyCode) dbRecord.CurrencyCode = currencyCode;

            var minorCurrencyCode = _chainConfig.MinorCurrencyCode.ToLower().Trim();
            if (dbRecord.MinorCurrencyCode != minorCurrencyCode) dbRecord.MinorCurrencyCode = minorCurrencyCode;

            var currencyPrecision = _chainConfig.CurrencyPrecision;
            if (dbRecord.CurrencyPrecision != currencyPrecision) dbRecord.CurrencyPrecision = currencyPrecision;

            var displayName = _chainConfig.InstanceDisplayName.Trim();
            if (dbRecord.InstanceDisplayName != displayName) dbRecord.InstanceDisplayName = displayName;

            var network = _chainConfig.Network.Trim();
            if (dbRecord.Network != network) dbRecord.Network = network;

            if (dbRecord.MonitorAddresses == null) dbRecord.MonitorAddresses = new List<MonitorAddress>();

            foreach (var address in _chainConfig.MonitorAddresses)
            {
                if (dbRecord.MonitorAddresses.Any(x => x.Address == address)) continue;

                dbRecord.MonitorAddresses.Add(new MonitorAddress { Address = address });
            }

            foreach (var address in dbRecord.MonitorAddresses.ToArray())
            {
                if (_chainConfig.MonitorAddresses.Any(x => x == address.Address)) continue;

                dbRecord.MonitorAddresses.Remove(address);
            }

            _appDbContext.SaveChanges();
        }

        private async Task UpdateDbRecordApiRespondingsAsync(ChainDbItem dbRecord)
        {
            // FULLNODE
            try
            {
                var fullNodeResult = await _fullNodeApiClient.GetNetworkInfoAsync();
                if (fullNodeResult == null || !fullNodeResult.Success)
                {
                    _logger.LogWarning("Communication with fullnode for {ChainName} {DisplayName} failed.", _chainConfig.ChainName.ToUpper(), _chainConfig.InstanceDisplayName);
                    dbRecord.IsFullNodeApiResponsive = false;
                }
                else
                {
                    dbRecord.IsFullNodeApiResponsive = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Communication with fullnode for {ChainName} {DisplayName} failed.", _chainConfig.ChainName.ToUpper(), _chainConfig.InstanceDisplayName);
                dbRecord.IsFullNodeApiResponsive = false;
            }

            //FARMER
            try
            {
                var farmerResult = await _farmerApiClient.GetNetworkInfoAsync();
                if (farmerResult == null || !farmerResult.Success)
                {
                    _logger.LogWarning("Communication with farmer for {ChainName} {DisplayName} failed.", _chainConfig.ChainName.ToUpper(), _chainConfig.InstanceDisplayName);
                    dbRecord.IsFarmerApiResponsive = false;
                }
                else
                {
                    dbRecord.IsFarmerApiResponsive = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Communication with farmer for {ChainName} {DisplayName} failed.", _chainConfig.ChainName.ToUpper(), _chainConfig.InstanceDisplayName);
                dbRecord.IsFarmerApiResponsive = false;
            }

            //HARVESTER
            try
            {
                var harvesterResult = await _harvesterApiClient.GetNetworkInfoAsync();
                if (harvesterResult == null || !harvesterResult.Success)
                {
                    _logger.LogWarning("Communication with harvester for {ChainName} {DisplayName} failed.", _chainConfig.ChainName.ToUpper(), _chainConfig.InstanceDisplayName);
                    dbRecord.IsHarvesterApiResponsive = false;
                }
                else
                {
                    dbRecord.IsHarvesterApiResponsive = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Communication with harvester for {ChainName} {DisplayName} failed.", _chainConfig.ChainName.ToUpper(), _chainConfig.InstanceDisplayName);
                dbRecord.IsHarvesterApiResponsive = false;
            }

            //WALLET
            try
            {
                var walletResult = await _walletApiClient.GetNetworkInfoAsync();
                if (walletResult == null || !walletResult.Success)
                {
                    if (_chainConfig.EnableWalletMonitoring)
                    {
                        _logger.LogWarning("Communication with wallet for {ChainName} {DisplayName} failed.", _chainConfig.ChainName.ToUpper(), _chainConfig.InstanceDisplayName);
                    }

                    dbRecord.IsWalletApiResponsive = false;
                }
                else
                {
                    dbRecord.IsWalletApiResponsive = true;
                }
            }
            catch (Exception ex)
            {
                if (_chainConfig.EnableWalletMonitoring)
                {
                    _logger.LogError(ex, "Communication with wallet for {ChainName} {DisplayName} failed.", _chainConfig.ChainName.ToUpper(), _chainConfig.InstanceDisplayName);
                }
                dbRecord.IsWalletApiResponsive = false;
            }
        }

        private async Task UpdateDbRecordFromBlockchainStateAsync(ChainDbItem dbRecord)
        {
            if (!dbRecord.IsFullNodeApiResponsive)
            {
                _logger.LogWarning("{ChainName} {DisplayName} API marked as unresponsive, skipping update from fullnode API.", _chainConfig.ChainName.ToUpper(), _chainConfig.InstanceDisplayName);
                return;
            }
            try
            {
                // FullNode status
                var chainState = await _fullNodeApiClient.GetBlockchainStateAsync();
                if (chainState != null && chainState.Success)
                {
                    if (!chainState.BlockchainState.Sync.Synced)
                    {
                        dbRecord.Status = Status.Syncing;
                        dbRecord.SyncedBlockHeight = chainState.BlockchainState.Sync.SyncProgressHeight;
                    }
                    else
                    {
                        dbRecord.SyncedBlockHeight = chainState.BlockchainState.Peak.Height;
                    }

                    dbRecord.TotalNetspace = (decimal)chainState.BlockchainState.Space;
                    dbRecord.Difficulty = chainState.BlockchainState.Difficulty;
                }
                else
                {
                    _logger.LogCritical("{ChainName} {DisplayName} API failure, {Err}.", _chainConfig.ChainName.ToUpper(), _chainConfig.InstanceDisplayName, chainState.Error);
                }

                // FullNode Peers
                if (dbRecord.Peers == null) dbRecord.Peers = new List<PeerDbItem>();

                var chainPeers = await _fullNodeApiClient.GetConnectionsAsync();
                if (chainPeers != null && chainPeers.Success)
                {
                    if (chainPeers.Connections?.Count > 0)
                    {
                        foreach (var peer in chainPeers.Connections)
                        {
                            if (dbRecord.Peers.Any(x => x.Host == peer.PeerHost.ToLower() && x.Port == peer.PeerPort)) continue;

                            dbRecord.Peers.Add(new PeerDbItem { Host = peer.PeerHost.ToLower(), Port = peer.PeerPort });
                        }

                        foreach (var peer in dbRecord.Peers.ToArray())
                        {
                            if (chainPeers.Connections.Any(x => x.PeerHost.ToLower() == peer.Host && x.PeerPort == peer.Port)) continue;

                            dbRecord.Peers.Remove(peer);
                        }
                    }
                    else
                    {
                        dbRecord.Peers.Clear();
                    }
                }
                else
                {
                    _logger.LogCritical("{ChainName} {DisplayName} API failure, {Err}.", _chainConfig.ChainName.ToUpper(), _chainConfig.InstanceDisplayName, chainPeers.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "{ChainName} {DisplayName} API failure.", _chainConfig.ChainName.ToUpper(), _chainConfig.InstanceDisplayName);
            }
        }

        private async Task UpdateDbRecordFromLogsAsync(ChainDbItem dbRecord)
        {
            await Task.CompletedTask;
            //TODO
            try
            {
                var debugLogFilePath = Path.Combine(FileSystem.GetChainLogsDirectoryPath(_chainConfig.ChainFolder, _chainConfig.Network), "debug.log");
                if (File.Exists(debugLogFilePath))
                {
                    var logItems = LogParser.Parser.ParseLines(debugLogFilePath, DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(dbRecord.LastLogTimestamp)));
                    if (logItems != null)
                    {
                        var lastLogTimestamp = dbRecord.LastLogTimestamp;
                        var eligiblePlots = 0u;
                        var proofs = 0u;
                        var filters = 0u;
                        var responseTimes = new List<double>();
                        try
                        {
                            foreach (var logItem in logItems)
                            {
                                lastLogTimestamp = Convert.ToUInt64(logItem.ProducedAt.ToUnixTimeMilliseconds());

                                if (logItem.LogLineType == LogParser.Models.LogLineType.EligiblePlots)
                                {
                                    var data = (HarvesterPlotsEligibleItem)logItem.Data;
                                    eligiblePlots += data.Plots;
                                    proofs += data.Proofs;
                                    filters++;
                                    responseTimes.Add(data.Time);
                                }
                            }

                            dbRecord.LastLogTimestamp = lastLogTimestamp;
                            dbRecord.LongestResponseTime = Convert.ToUInt32(TimeSpan.FromSeconds(responseTimes.Max()).TotalMilliseconds);
                            dbRecord.ShortestResponseTime = Convert.ToUInt32(TimeSpan.FromSeconds(responseTimes.Min()).TotalMilliseconds);
                            dbRecord.AvgResponseTime = Convert.ToUInt32(TimeSpan.FromSeconds(responseTimes.Average()).TotalMilliseconds);


                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to get/parse log items for {ChainName} {DisplayName}.", _chainConfig.ChainName.ToUpper(), _chainConfig.InstanceDisplayName);
                        }

                        dbRecord.LastLogTimestamp = lastLogTimestamp;
                    }
                }
                else
                {
                    _logger.LogWarning("Debug log file for {ChainName} {DisplayName} doesn't exist.", _chainConfig.ChainName.ToUpper(), _chainConfig.InstanceDisplayName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "{ChainName} {DisplayName} log failure.", _chainConfig.ChainName.ToUpper(), _chainConfig.InstanceDisplayName);
            }
        }
    }
}
