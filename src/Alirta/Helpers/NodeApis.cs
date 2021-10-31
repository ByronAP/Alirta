using Alirta.Contracts;
using Alirta.Models;
using ChiaApi;
using System.IO;

namespace Alirta.Helpers
{
    internal static class NodeApis
    {
        internal static FullNodeApiClient GetFullNodeApiClient(IChainConfig chainConfig)
        {
            var certsPath = FileSystem.GetChainCertDirectoryPath(chainConfig.ChainFolder, chainConfig.Network, NodeEntryPoint.Full_Node);
            var certFilePath = Path.Combine(certsPath, "private_full_node.crt");
            var keyFilePath = Path.Combine(certsPath, "private_full_node.key");

            var nodeApiConfig = new ChiaApiConfig(certFilePath, keyFilePath, "localhost", chainConfig.FullNodePort, null, 2, null, false);
            return new FullNodeApiClient(nodeApiConfig);
        }

        internal static FarmerApiClient GetFarmerApiClient(IChainConfig chainConfig)
        {
            var certsPath = FileSystem.GetChainCertDirectoryPath(chainConfig.ChainFolder, chainConfig.Network, NodeEntryPoint.Farmer);
            var certFilePath = Path.Combine(certsPath, "private_farmer.crt");
            var keyFilePath = Path.Combine(certsPath, "private_farmer.key");

            var nodeApiConfig = new ChiaApiConfig(certFilePath, keyFilePath, "localhost", chainConfig.FarmerPort, null, 2, null, false);
            return new FarmerApiClient(nodeApiConfig);
        }

        internal static HarvesterApiClient GetHarvesterApiClient(IChainConfig chainConfig)
        {
            var certsPath = FileSystem.GetChainCertDirectoryPath(chainConfig.ChainFolder, chainConfig.Network, NodeEntryPoint.Harvester);
            var certFilePath = Path.Combine(certsPath, "private_harvester.crt");
            var keyFilePath = Path.Combine(certsPath, "private_harvester.key");

            var nodeApiConfig = new ChiaApiConfig(certFilePath, keyFilePath, "localhost", chainConfig.HarvesterPort, null, 2, null, false);
            return new HarvesterApiClient(nodeApiConfig);
        }

        internal static WalletApiClient GetWalletApiClient(IChainConfig chainConfig)
        {
            var certsPath = FileSystem.GetChainCertDirectoryPath(chainConfig.ChainFolder, chainConfig.Network, NodeEntryPoint.Wallet);
            var certFilePath = Path.Combine(certsPath, "private_wallet.crt");
            var keyFilePath = Path.Combine(certsPath, "private_wallet.key");

            var nodeApiConfig = new ChiaApiConfig(certFilePath, keyFilePath, "localhost", chainConfig.WalletPort, null, 2, null, false);
            return new WalletApiClient(nodeApiConfig);
        }
    }
}
