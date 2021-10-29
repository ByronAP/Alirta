using Alirta.Contracts;
using Alirta.DbContexts;
using Alirta.Helpers;
using Alirta.Models;
using ChiaApi;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
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
            var dbRecordExists = _appDbContext.ChainItems.Find(_chainConfig.ChainName) != null;
            if (!dbRecordExists)
            {
                await InitDbRecord();
                return;
            }

            await Task.CompletedTask;
        }

        private async Task InitDbRecord()
        {
            var newDbRecord = new ChainDbItem();
            newDbRecord.ChainName = _chainConfig.ChainName;

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

            try
            {
                var chainState = await _fullNodeApiClient.GetBlockchainStateAsync();
                if (chainState != null && chainState.Success)
                {
                    if (!chainState.BlockchainState.Sync.Synced)
                    {
                        newDbRecord.Status = Status.Syncing;
                        newDbRecord.SyncedBlockHeight = chainState.BlockchainState.Sync.SyncProgressHeight;
                    }
                    else
                    {
                        newDbRecord.SyncedBlockHeight = chainState.BlockchainState.Peak.Height;
                    }
                }
                else
                {
                    _logger.LogCritical("{ChainName} API failure, {Err}.", _chainConfig.ChainName, chainState.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "{ChainName} API failure.", _chainConfig.ChainName);
                return;
            }

            _appDbContext.ChainItems.Add(newDbRecord);

            await _appDbContext.SaveChangesAsync();
        }
    }
}
