using System;
using System.Reflection;

namespace Alirta.Helpers
{
    internal static class Constants
    {
        internal const string AppName = "Alirta";
        internal const string AppConfigFileName = "app.config";
        internal const string DataBackupFolderName = "backups";
        internal const string DataFolderName = "data";
        internal const string DataFileName = "data.dat";
        internal const string ChainsFolderName = "chains";

        internal static string AppRootPath => Assembly.GetExecutingAssembly().Location;
        internal static string UserProfilePath => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    }
}
