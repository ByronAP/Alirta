using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Alirta.DbContexts
{
    internal class AppDbContext : DbContext
    {
        private string AppRootFolder => Assembly.GetExecutingAssembly().Location;

        private const string DataBackupFolderName = "backups";
        private const string DataFolderName = "data";
        private const string DataFileName = "data.dat";

        private readonly ILogger<AppDbContext> _logger;


        public AppDbContext()
        {
            if (!MigrateDbAsync().Result) Environment.Exit(2);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            var sqlDatabaseFullPath = Path.Combine(AppRootFolder, DataFolderName, DataFileName);

            _ = optionsBuilder.UseSqlite($"Filename={sqlDatabaseFullPath}", options =>
            {
                _ = options.CommandTimeout(15);
            });

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
        }

        private async Task<bool> MigrateDbAsync()
        {
            if (Database.GetPendingMigrations().Any())
            {
                try
                {
                    _logger?.LogInformation("Database has missing migrations or doesn't exist, applying migrations.");
                    await Database.MigrateAsync();
                    _logger?.LogInformation("Database migrations applied.");
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Failed to apply migrations, db is incompatible.");

                    _logger?.LogWarning(ex, "Attempting to retrieve existing accounts before db deletion.");
                    //Account[] accounts = null;
                    //try
                    //{
                    //    accounts = Accounts.ToArray();
                    //}
                    //catch (Exception exx)
                    //{
                    //    _logger?.LogError(exx, "Failed to retrieve accounts.");
                    //}

                    _logger?.LogWarning("Deleting database due to incompatability.");

                    // create a backup file
                    BackupDbFile();

                    var isDeleted = await Database.EnsureDeletedAsync();
                    if (isDeleted)
                    {
                        _logger?.LogWarning("Database delete success.");
                        _logger?.LogWarning("Attempting to create new db.");
                        try
                        {
                            await Database.MigrateAsync();
                        }
                        catch (Exception exx)
                        {
                            _logger?.LogError(exx, "Failed to create new db.");
                            return false;
                        }

                        _logger?.LogWarning("Attempting to migrate accounts to new db.");
                        try
                        {
                            //if (accounts != null)
                            //{
                            //    foreach (var account in accounts)
                            //    {
                            //        try
                            //        {
                            //            _ = Accounts.Add(account);
                            //            await SaveChangesAsync();
                            //        }
                            //        catch (Exception exx)
                            //        {
                            //            _logger?.LogError(exx, "Faild to migrate account {ID}: {DisplayName}", account.Id, account.DisplayName);
                            //        }
                            //    }
                            //}
                        }
                        catch (Exception exx)
                        {
                            _logger?.LogError(exx, "Failed to migrate accounts.");
                        }

                        return true;
                    }
                    else
                    {
                        _logger?.LogError("Database delete failed.");
                    }

                    return false;
                }
            }

            return true;
        }

        public bool BackupDbFile()
        {
            try
            {
                Database.CloseConnection();

                var sourceSqlDatabaseFullPath = Path.Combine(AppRootFolder, DataFolderName, DataFileName);
                var destinationSqlDatabaseDirectory = Path.Combine(AppRootFolder, DataFolderName, DataBackupFolderName);
                var destinationSqlDatabaseFullPath = Path.Combine(destinationSqlDatabaseDirectory, $"db-backup-{DateTimeOffset.Now.ToUnixTimeSeconds()}.bak");

                if (!Directory.Exists(destinationSqlDatabaseDirectory)) Directory.CreateDirectory(destinationSqlDatabaseDirectory);

                File.Copy(sourceSqlDatabaseFullPath, destinationSqlDatabaseFullPath, true);

                return File.Exists(destinationSqlDatabaseFullPath);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to backup database.");
                return false;
            }
        }
    }
}
