using LogParser.Models;
using Microsoft.Extensions.Logging;

namespace LogParser.Helpers
{
    public static class Extensions
    {
        /// <summary>
        /// Convert string to LogLevel.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The LogLevel.</returns>
        public static LogLevel ToLogLevel(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return LogLevel.None;

            // some of these are not used but we add the cases anyway
            return value.Trim().ToLower() switch
            {
                "warning" => LogLevel.Warning,
                "warn" => LogLevel.Warning,
                "error" => LogLevel.Error,
                "information" => LogLevel.Information,
                "info" => LogLevel.Information,
                "debug" => LogLevel.Debug,
                "trace" => LogLevel.Trace,
                "critical" => LogLevel.Critical,
                _ => LogLevel.None
            };
        }

        public static LogLineProducer ToLogLineProducer(this string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return LogLineProducer.Unknown;

            return value.Trim().ToLower() switch
            {
                "full node" => LogLineProducer.FullNode,
                "full_node" => LogLineProducer.FullNode,
                "fullnode" => LogLineProducer.FullNode,
                "harvester" => LogLineProducer.Harvester,
                "wallet" => LogLineProducer.Wallet,
                "farmer" => LogLineProducer.Farmer,
                "daemon" => LogLineProducer.Daemon,
                _ => LogLineProducer.Unknown
            };
        }
    }
}
