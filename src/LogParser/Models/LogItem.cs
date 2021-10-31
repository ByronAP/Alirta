using LogParser.Helpers;
using Microsoft.Extensions.Logging;
using System;

namespace LogParser.Models
{
    public class LogItem
    {
        public uint Index { get; set; }
        public LogLineType LogLineType { get; set; }
        public DateTimeOffset ProducedAt { get; set; }
        public LogLineProducer Producer { get; set; }
        public string ProducerLocation { get; set; }
        public LogLevel LineLogLevel { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public static LogItem FromString(string line, uint index = 0)
        {
            const char Seperator = ' ';

            if (!line.StartsWith("20")) throw new Exception("Invalid line.");

            /* ------------ LINE FORMAT ------------
             * time producer location loglevel message
             */

            // time
            var timeSepLocation = line.IndexOf(Seperator);
            if (timeSepLocation <= 0 || timeSepLocation > 25) throw new Exception("Invalid line");
            var timeString = line.Substring(0, timeSepLocation).Trim();

            // producer
            var producerSepLocation = line.IndexOf(Seperator, timeSepLocation + 1);
            var producerString = line.Substring(timeSepLocation + 1, producerSepLocation - timeSepLocation).Trim();

            // location
            var locationSepLocation = line.IndexOf(Seperator, producerSepLocation + 1);
            var locationString = line.Substring(producerSepLocation + 1, locationSepLocation - producerSepLocation);
            locationString = locationString.Replace(":", "").Trim();

            // Log level
            var logLevelSepLocation = line.IndexOf(Seperator, locationSepLocation + 1);
            var logLevelString = line.Substring(locationSepLocation + 1, logLevelSepLocation - locationSepLocation).Trim();

            // Message
            var messageString = line.Substring(logLevelSepLocation + 1).Trim();

            var logLineType = LogLineType.Unknown;
            var data = new Object();
            switch (producerString.ToLogLineProducer())
            {
                //TODO
                case LogLineProducer.Harvester:
                    if (RegeX.IsHarvesterPlotsEligibleItem(messageString))
                    {
                        logLineType = LogLineType.EligiblePlots;
                        data = messageString.ToHarvesterPlotsEligibleItem();
                    }
                    break;
                default:
                    logLineType = LogLineType.Unknown;
                    break;

            }

            // now that we have all the parts lets build the result object
            var result = new LogItem
            {
                ProducedAt = DateTimeOffset.Parse(timeString),
                Producer = producerString.ToLogLineProducer(),
                ProducerLocation = locationString,
                LineLogLevel = logLevelString.ToLogLevel(),
                Message = messageString,
                Index = index,
                LogLineType = logLineType,
                Data = data
            };

            return result;
        }
    }
}
