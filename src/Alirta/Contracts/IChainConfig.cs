namespace Alirta.Contracts
{
    public interface IChainConfig
    {
        string ChainName { get; set; }

        string MajorCurrencyName { get; set; }

        string MinorCurrencyName { get; set; }

        uint DecimalLocation { get; set; }

        string ExecutableName { get; set; }

        string ChainFolder { get; set; }

        string AppFolder { get; set; }

        string Network { get; set; }

        uint FarmerPort { get; set; }

        uint FullNodePort { get; set; }

        uint HarvesterPort { get; set; }

        uint WalletPort { get; set; }
    }
}
