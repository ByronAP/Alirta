using Alirta.Helpers;
using Alirta.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace Alirta
{
    class Program
    {
        internal T GetService<T>()
            where T : class
            => _host.Services.GetService(typeof(T)) as T;


        private static IHost _host;

        static async Task Main()
        {
            Console.WriteLine($"{DateTimeOffset.UtcNow} INFO: App starting in {FileSystem.AppDirectoryPath}");

            await EnsureAppConfigExistsAsync();

            _host = Host.CreateDefaultBuilder()
                   .ConfigureAppConfiguration(c =>
                   {
                       c.SetBasePath(FileSystem.AppDirectoryPath);
                   })
                   .ConfigureServices(ConfigureServices)
                   .Build();

            await _host.StartAsync();

            await _host.WaitForShutdownAsync();
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.Configure<AppConfig>(context.Configuration);
        }

        private static async Task EnsureAppConfigExistsAsync()
        {
            try
            {
                var appConfigPath = Path.Combine(FileSystem.AppDirectoryPath, Constants.AppConfigFileName);
                if (!File.Exists(appConfigPath))
                {
                    Console.WriteLine($"{DateTimeOffset.UtcNow} INFO: Creating default config file (app.config). You need to edit this file with your own settings.");
                    var newAppConfig = new AppConfig();
                    var appConfigJson = JsonSerializer.Serialize(newAppConfig);

                    await File.WriteAllTextAsync(appConfigPath, appConfigJson);

                    await Task.Delay(5000);

                    Environment.Exit(2);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTimeOffset.UtcNow} Error: Failed to create default app.config file. Please ensure file exists or permission to create file is set. Ex: {ex.Message}");

                await Task.Delay(5000);

                Environment.Exit(2);
            }
        }
    }
}
