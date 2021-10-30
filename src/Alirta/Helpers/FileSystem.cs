using Alirta.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Alirta.Helpers
{
    internal static class FileSystem
    {
        internal static string GetChainCertsDirectoryRootPath(string chainFolder, string network)
        {
            return Path.Combine(Constants.UserProfilePath, chainFolder, network, "config", "ssl");
        }

        internal static string GetChainCertDirectoryPath(string chainFolder, string network, NodeEntryPoint nodeEntryPoint)
        {
            var certsPath = GetChainCertsDirectoryRootPath(chainFolder, network);
            return Path.Combine(certsPath, nodeEntryPoint.ToNodeAppName());
        }

        internal static string GetChainLogsDirectoryPath(string chainFolder, string network)
        {
            return Path.Combine(Constants.UserProfilePath, chainFolder, network, "log");
        }

        internal static string[] ListChainConfigs()
        {
            var chainsPath = Path.Combine(Constants.AppRootPath, Constants.ChainsFolderName);

            return Directory.EnumerateFiles(chainsPath, "*.config").ToArray();
        }

        internal static IEnumerable<ChainConfig> GetChainConfigs()
        {
            foreach (var fileName in ListChainConfigs())
            {
                var jsonString = File.ReadAllText(fileName);
                var chainItem = ChainConfig.FromJson(jsonString);
                chainItem.ConfigFilePath = fileName;

                yield return chainItem;
            }
        }
    }
}
