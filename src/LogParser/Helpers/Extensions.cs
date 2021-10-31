using LogParser.Models;
using Microsoft.Extensions.Logging;
using System;

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

        public static HarvesterPlotsEligibleItem ToHarvesterPlotsEligibleItem(this string value)
        {
            var plotsSep = value.IndexOf(" ");
            var plotsStr = value.Substring(0, plotsSep);

            var proofsSep = value.IndexOf("found ", StringComparison.InvariantCultureIgnoreCase) + 6;
            var proofsEndSep = value.IndexOf(" ", proofsSep) - proofsSep;
            var proofsStr = value.Substring(proofsSep, proofsEndSep);

            var timeSep = value.IndexOf("time: ", StringComparison.InvariantCultureIgnoreCase) + 6;
            var timeEndSep = value.IndexOf(" ", timeSep) - timeSep;
            var timeStr = value.Substring(timeSep, timeEndSep);

            var totalSep = value.IndexOf("total ", StringComparison.InvariantCultureIgnoreCase) + 6;
            var totalEndSep = value.IndexOf(" ", totalSep) - totalSep;
            var totalStr = value.Substring(totalSep, totalEndSep);

            var plots = uint.Parse(plotsStr);
            var proofs = uint.Parse(proofsStr);
            var plotsTotal = uint.Parse(totalStr);
            var time = double.Parse(timeStr);

            // chains do not seem to report properly below 0.01S so  we just set a hard min
            if (time < 0.01d) time = 0.01d;

            return new HarvesterPlotsEligibleItem { Plots = plots, Proofs = proofs, PlotsTotal = plotsTotal, Time = time };
        }
    }
}
