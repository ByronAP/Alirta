using Alirta.Helpers;
using Alirta.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Alirta.DbContexts
{
    internal class AppDbContext : DbContext
    {
        public DbSet<ChainDbItem> ChainItems { get; set; }

#nullable enable
        private readonly ILogger<AppDbContext>? _logger;
#nullable restore

        public AppDbContext()
        {
            if (!MigrateDbAsync().Result) Environment.Exit(2);

            _logger = null;
        }

        public AppDbContext(ILogger<AppDbContext> logger)
        {
            _logger = logger;

            if (!MigrateDbAsync().Result) Environment.Exit(2);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            var sqlDatabaseFullPath = Path.Combine(Constants.AppRootPath, Constants.DataFolderName, Constants.DataFileName);

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

                    // create a backup file
                    BackupDbFile();

                    _logger?.LogWarning("Deleting database due to incompatability.");

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
                _logger?.LogWarning("Database backup started.");

                Database.CloseConnection();

                var sourceSqlDatabaseFullPath = Path.Combine(Constants.AppRootPath, Constants.DataFolderName, Constants.DataFileName);
                var destinationSqlDatabaseDirectory = Path.Combine(Constants.AppRootPath, Constants.DataFolderName, Constants.DataBackupFolderName);
                var destinationSqlDatabaseFullPath = Path.Combine(destinationSqlDatabaseDirectory, $"db-backup-{DateTimeOffset.Now.ToUnixTimeSeconds()}.bak");

                if (!Directory.Exists(destinationSqlDatabaseDirectory)) Directory.CreateDirectory(destinationSqlDatabaseDirectory);

                File.Copy(sourceSqlDatabaseFullPath, destinationSqlDatabaseFullPath, true);

                var fileExists = File.Exists(destinationSqlDatabaseFullPath);

                if (fileExists)
                {
                    _logger?.LogWarning("Database backup completed.");
                }
                else
                {
                    _logger?.LogError("Database backup failed, backup not found.");
                }

                return fileExists;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Database backup failed from exception.");
                return false;
            }
        }
    }
}
