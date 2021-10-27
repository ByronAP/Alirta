using System;
using System.IO;
using System.Reflection;

namespace Alirta.Helpers
{
    internal static class FileSystem
    {
        internal static string AppDirectoryPath { get => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location); }

        internal static string UserProfilePath => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        internal static string GetCertsDirectoryRootPath(string chainFolder, string network)
        {
            return Path.Combine(UserProfilePath, chainFolder, network, "config", "ssl");
        }

        internal static string GetCertDirectoryPath(string chainFolder, string network, NodeApp nodeApp)
        {
            var certsPath = GetCertsDirectoryRootPath(chainFolder, network);
            return Path.Combine(certsPath, nodeApp.ToString().ToLowerInvariant());
        }

        internal static string GetLogsDirectoryPath(string chainFolder, string network)
        {
            return Path.Combine(UserProfilePath, chainFolder, network, "log");
        }
    }
}
