using System;
using System.IO;
using System.Reflection;

namespace Alirta.Helpers
{
    internal static class Constants
    {
        internal const string AppName = "Alirta";
        internal const uint ServerUpdateIntervalMinutes = 5;
        internal const string AppConfigFileName = "app.config";
        internal const string DataBackupFolderName = "backups";
        internal const string DataFolderName = "data";
        internal const string DataFileName = "data.dat";
        internal const string ChainsFolderName = "chains";

        internal static string AppRootPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        internal static string UserProfilePath => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        internal static string ChainConfigsPath => Path.Combine(AppRootPath, ChainsFolderName);
        internal static string AppConfigFilePath => Path.Combine(AppRootPath, AppConfigFileName);
        internal static string DatabasePath => Path.Combine(AppRootPath, DataFolderName);
        internal static string DatabaseFilePath => Path.Combine(DatabasePath, DataFileName);
        internal static string DatabaseBackupPath = Path.Combine(DatabasePath, DataBackupFolderName);
    }
}
