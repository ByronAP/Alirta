using Microsoft.Extensions.Logging;

namespace Alirta.Models
{
    public class AppConfig
    {
        public string ServiceKey { get; set; } = string.Empty;

        public LogLevel LogLevel { get; set; } = LogLevel.Warning;

        public bool LogToFile { get; set; } = false;

        public string ChainsDirectory { get; set; } = "blockchains";

        public bool RemoteControlEnabled { get; set; } = false;

        public string RemoteControlPassword { get; set; } = string.Empty;
    }
}
