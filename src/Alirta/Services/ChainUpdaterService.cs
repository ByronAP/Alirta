using Alirta.Contracts;
using Alirta.DbContexts;
using Alirta.Helpers;
using Alirta.Models;
using ChiaApi;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Alirta.Services
{
    internal class ChainUpdaterService : IHostedService
    {
        private readonly IChainConfig _chainConfig;
        private readonly ILogger<ChainUpdaterService> _logger;
        private readonly AppDbContext _appDbContext;
        private Timer _timer;
        private readonly FullNodeApiClient _fullNodeApiClient;
        private readonly FarmerApiClient _farmerApiClient;
        private readonly HarvesterApiClient _harvesterApiClient;
        private readonly WalletApiClient _walletApiClient;

        public ChainUpdaterService(IChainConfig chainConfig, ILogger<ChainUpdaterService> logger, AppDbContext appDbContext)
        {
            _chainConfig = chainConfig;
            _logger = logger;
            _appDbContext = appDbContext;

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

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            var dbRecord = await _appDbContext.ChainItems.FindAsync(_chainConfig.ChainName);
            if (dbRecord == null)
            {
                dbRecord = await InitDbRecordAsync();
            }

            await UpdateDbRecordAsync(dbRecord);

            if (DateTimeOffset.FromUnixTimeMilliseconds(Convert.ToInt64(dbRecord.LastSubmittedTimestamp)).AddMinutes(Constants.ServerUpdateIntervalMinutes) < DateTimeOffset.UtcNow)
            {
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
                        _logger.LogError(ex, "Failed to get/parse initial log item for {ChainName}.", _chainConfig.ChainName);
                    }
                }
            }
            else
            {
                _logger.LogWarning("Debug log file for {ChainName} doesn't exist.", _chainConfig.ChainName);
            }

            await _appDbContext.SaveChangesAsync();

            return newDbRecord;
        }

        private async Task UpdateDbRecordAsync(ChainDbItem dbRecord)
        {
            UpdateDbRecordFromChainConfig(dbRecord);

            // check the apis before we bother trying to work with them
            await UpdateDbRecordApiRespondingsAsync(dbRecord);

            var tasks = new List<Task>();
            tasks.Add(UpdateDbRecordFromBlockchainStateAsync(dbRecord));

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

            dbRecord.MonitorAddresses = new List<MonitorAddress>();
            if (_chainConfig.MonitorAddresses != null)
            {
                var monitoredAddresses = new List<MonitorAddress>();
                foreach (var address in _chainConfig.MonitorAddresses)
                {
                    monitoredAddresses.Add(new MonitorAddress { Address = address });
                }

                dbRecord.MonitorAddresses = monitoredAddresses;
            }
        }

        private async Task UpdateDbRecordApiRespondingsAsync(ChainDbItem dbRecord)
        {
            // FULLNODE
            try
            {
                var fullNodeResult = await _fullNodeApiClient.GetNetworkInfoAsync();
                if (fullNodeResult == null || !fullNodeResult.Success)
                {
                    _logger.LogWarning("Communication with fullnode for {ChainName} {ChainDisplayName} failed.", _chainConfig.ChainName, _chainConfig.InstanceDisplayName);
                    dbRecord.IsFullNodeApiResponsive = false;
                }
                else
                {
                    dbRecord.IsFullNodeApiResponsive = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Communication with fullnode for {ChainName} {ChainDisplayName} failed.", _chainConfig.ChainName, _chainConfig.InstanceDisplayName);
                dbRecord.IsFullNodeApiResponsive = false;
            }

            //FARMER
            try
            {
                var farmerResult = await _farmerApiClient.GetNetworkInfoAsync();
                if (farmerResult == null || !farmerResult.Success)
                {
                    _logger.LogWarning("Communication with farmer for {ChainName} {ChainDisplayName} failed.", _chainConfig.ChainName, _chainConfig.InstanceDisplayName);
                    dbRecord.IsFarmerApiResponsive = false;
                }
                else
                {
                    dbRecord.IsFarmerApiResponsive = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Communication with farmer for {ChainName} {ChainDisplayName} failed.", _chainConfig.ChainName, _chainConfig.InstanceDisplayName);
                dbRecord.IsFarmerApiResponsive = false;
            }

            //HARVESTER
            try
            {
                var harvesterResult = await _harvesterApiClient.GetNetworkInfoAsync();
                if (harvesterResult == null || !harvesterResult.Success)
                {
                    _logger.LogWarning("Communication with harvester for {ChainName} {ChainDisplayName} failed.", _chainConfig.ChainName, _chainConfig.InstanceDisplayName);
                    dbRecord.IsHarvesterApiResponsive = false;
                }
                else
                {
                    dbRecord.IsHarvesterApiResponsive = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Communication with harvester for {ChainName} {ChainDisplayName} failed.", _chainConfig.ChainName, _chainConfig.InstanceDisplayName);
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
                        _logger.LogWarning("Communication with wallet for {ChainName} {ChainDisplayName} failed.", _chainConfig.ChainName, _chainConfig.InstanceDisplayName);
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
                    _logger.LogError(ex, "Communication with wallet for {ChainName} {ChainDisplayName} failed.", _chainConfig.ChainName, _chainConfig.InstanceDisplayName);
                }
                dbRecord.IsWalletApiResponsive = false;
            }
        }

        private async Task UpdateDbRecordFromBlockchainStateAsync(ChainDbItem dbRecord)
        {
            try
            {
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
                    _logger.LogCritical("{ChainName} API failure, {Err}.", _chainConfig.ChainName, chainState.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "{ChainName} API failure.", _chainConfig.ChainName);
            }
        }
    }
}
