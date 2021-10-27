using Alirta.Contracts;

namespace Alirta.Models
{
    public class ChainConfig : IChainConfig
    {
        public string ExecutableName { get; set; } = "chia";

        public string ChainFolder { get; set; } = ".chia";

        public string AppFolder { get; set; } = "chia-blockchain";

        public string Network { get; set; } = "mainnet";

        public uint DaemonPort { get; set; } = 55400;

        public uint FarmerPort { get; set; } = 8559;

        public uint FullNodePort { get; set; } = 8555;

        public uint HarvesterPort { get; set; } = 8560;

        public uint WalletPort { get; set; } = 9256;
    }
}
