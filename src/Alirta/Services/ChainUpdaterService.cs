using Alirta.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace Alirta.Services
{
    internal class ChainUpdaterService : IHostedService
    {
        private readonly IChainConfig _chainConfig;
        private readonly ILogger<ChainUpdaterService> _logger;

        public ChainUpdaterService(IChainConfig chainConfig, ILogger<ChainUpdaterService> logger)
        {
            _chainConfig = chainConfig;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
