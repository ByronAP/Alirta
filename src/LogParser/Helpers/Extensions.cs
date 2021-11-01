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

            // chains do not seem to report below 0.01S so we just set a hard min
            if (time < 0.01d) time = 0.01d;

            return new HarvesterPlotsEligibleItem { Plots = plots, Proofs = proofs, PlotsTotal = plotsTotal, Time = time };
        }

        public static FarmedUnfinishedBlockItem ToFarmedUnfinishedBlockItem(this string value)
        {
            value = value.Trim();
            var farmedSep = value.IndexOf(" ") + 1;
            var unfinishedSep = value.IndexOf(" ", farmedSep) + 1;
            var blockSep = value.IndexOf(", ", unfinishedSep);
            var blockStr = value.Substring(unfinishedSep, blockSep - unfinishedSep);

            var spSep = value.IndexOf(" ", blockSep) + 1;
            var spEndSep = value.IndexOf(" ", spSep) + 1;
            var spValSep = value.IndexOf(", ", spEndSep);
            var spValStr = value.Substring(spEndSep, spValSep - spEndSep);

            var validationSep = value.IndexOf(" ", spValSep + 1) + 1;
            var timeSep = value.IndexOf(": ", validationSep) + 1;
            var timeEndSep = value.IndexOf(", ", timeSep);
            var timeStr = value.Substring(timeSep, timeEndSep - timeSep);

            var costSep = value.IndexOf(": ", timeEndSep + 1) + 1;
            var costStr = value.Substring(costSep).Trim();

            var cost = ulong.Parse(costStr);
            var sp = uint.Parse(spValStr);
            var time = double.Parse(timeStr);

            return new FarmedUnfinishedBlockItem { Block = blockStr, Cost = cost, SP = sp, ValidationTime = time };
        }
    }
}
