﻿using System.Collections.Generic;

namespace Alirta.Contracts
{
    public interface IChainConfig
    {
        uint Id { get; set; }

        string ChainName { get; set; }

        string InstanceDisplayName { get; set; }

        string CurrencyCode { get; set; }

        string MinorCurrencyCode { get; set; }

        uint CurrencyPrecision { get; set; }

        decimal BlockReward { get; set; }

        uint BlocksPer10Min { get; set; }

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

        string ConfigFilePath { get; set; }

        void Save();
    }
}
