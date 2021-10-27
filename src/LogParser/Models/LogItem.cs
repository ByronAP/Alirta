using LogParser.Helpers;
using Microsoft.Extensions.Logging;
using System;

namespace LogParser.Models
{
    public class LogItem
    {
        public DateTimeOffset ProducedAt { get; set; }
        public LogLineProducer Producer { get; set; }
        public string ProducerLocation { get; set; }
        public LogLevel LineLogLevel { get; set; }
        public string Message { get; set; }

        public static LogItem FromString(string line)
        {
            /* ------------ LINE FORMAT ------------
             * time producer location : loglevel message
             */

            // since the time and the message can contain ':'
            // we have to find the first location  of ' :'
            var sepLocation = line.IndexOf(" :");
            // what was before the ' :'
            var part1 = line.Substring(0, sepLocation).Trim();
            // what was after the ' :'
            var part2 = line.Substring(sepLocation + 2).Trim();

            // part 1 should now be 'time producer location'
            var part1parts = part1.Split(' ');
            var timeString = part1parts[0].Trim();
            var producerString = part1parts[1].Trim();
            var locationString = part1parts[2].Trim();

            // part 2 should now be 'loglevel message'
            // since the message is not quoted and can contain spaces
            // we have to find the location of the space between parts
            sepLocation = part2.IndexOf(' ');
            var typeString = part2.Substring(0, sepLocation);
            var messageString = part2.Substring(sepLocation);

            // now that we have all the parts lets build the result object
            var result = new LogItem
            {
                ProducedAt = DateTimeOffset.Parse(timeString),
                Producer = producerString.ToLogLineProducer(),
                ProducerLocation = locationString,
                LineLogLevel = typeString.ToLogLevel(),
                Message = messageString
            };

            return result;
        }
    }
}
