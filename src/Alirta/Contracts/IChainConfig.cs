using System.Collections.Generic;

namespace Alirta.Contracts
{
    public interface IChainConfig
    {
        string ChainName { get; set; }

        string InstanceDisplayName { get; set; }

        string CurrencyCode { get; set; }

        string MinorCurrencyCode { get; set; }

        uint CurrencyPrecision { get; set; }

        string ExecutableName { get; set; }

        string ChainFolder { get; set; }

        string AppFolder { get; set; }

        string Network { get; set; }

        uint FarmerPort { get; set; }

        uint FullNodePort { get; set; }

        uint HarvesterPort { get; set; }

        uint WalletPort { get; set; }

        bool EnableWalletMonitoring { get; set; }

        List<string> MonitorAddresses { get; set; }
    }
}
