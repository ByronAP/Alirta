using Alirta.DbContexts;
using Alirta.Helpers;
using Alirta.Models;
using Alirta.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Alirta
{
    class Program
    {
        internal static T GetService<T>()
            where T : class
            => _host.Services.GetService(typeof(T)) as T;


        private static IHost _host;

        static async Task Main()
        {
            Console.WriteLine($"{DateTimeOffset.UtcNow} INFO: App starting in {Constants.AppRootPath}");

            EnsureRequiredDirectoriesExist();

            await EnsureAppConfigExistsAsync();

            _host = Host.CreateDefaultBuilder()
                   .ConfigureAppConfiguration(c =>
                   {
                       c.SetBasePath(Constants.AppRootPath);
                   })
                   .ConfigureServices(ConfigureServices)
                   .Build();

            await _host.StartAsync();

            await _host.WaitForShutdownAsync();
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.Configure<AppConfig>(context.Configuration);
            services.AddDbContext<AppDbContext>();
            services.AddLogging();
            var chainCount = 0;
            foreach (var config in FileSystem.GetChainConfigs())
            {
                services.AddHostedService(sp =>
                new ChainUpdaterService(
                    config,
                    sp.GetService<ILogger<ChainUpdaterService>>(),
                    sp.GetService<AppDbContext>()
                    )
                );
                chainCount++;
            }

            if (chainCount <= 0)
            {
                Console.WriteLine($"{DateTimeOffset.UtcNow} ERROR: No chain configs found (ex: chia.config). Please create your chain configs in the 'chains' folder before starting this service.");
                Task.Delay(5000).Wait();
                Environment.Exit(2);
            }
        }

        private static void EnsureRequiredDirectoriesExist()
        {
            if (!Directory.Exists(Constants.ChainConfigsPath))
            {
                Directory.CreateDirectory(Constants.ChainConfigsPath);

                // add the default Chia config
                var chainConfig = new ChainConfig();
                var chainConfigJson = JsonSerializer.Serialize(chainConfig);

                try
                {
                    File.WriteAllText(Path.Combine(Constants.ChainConfigsPath, "chia.config.template"), chainConfigJson);
                }
                catch
                {
                    // ignore
                }
            }

            if (!Directory.Exists(Constants.DatabasePath))
            {
                Directory.CreateDirectory(Constants.DatabasePath);
            }
        }

        private static async Task EnsureAppConfigExistsAsync()
        {
            try
            {
                if (!File.Exists(Constants.AppConfigFilePath))
                {
                    Console.WriteLine($"{DateTimeOffset.UtcNow} ERROR: Creating default config file (app.config). You need to edit this file with your own settings before starting this service.");
                    var newAppConfig = new AppConfig();
                    var appConfigJson = JsonSerializer.Serialize(newAppConfig);

                    await File.WriteAllTextAsync(Constants.AppConfigFilePath, appConfigJson);

                    await Task.Delay(5000);

                    Environment.Exit(2);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTimeOffset.UtcNow} ERROR: Failed to create default 'app.config' file. Please ensure file exists or permission to create a file / write is set. Ex: {ex.Message}");

                await Task.Delay(5000);

                Environment.Exit(2);
            }
        }
    }
}
