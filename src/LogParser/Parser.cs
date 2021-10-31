using LogParser.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LogParser
{
    public static class Parser
    {
        public static IEnumerable<LogItem> ParseLines(string fileName, uint startLine = uint.MinValue, uint endLine = uint.MaxValue)
        {
            var currentLine = uint.MinValue;

            foreach (var line in File.ReadLines(fileName))
            {
                currentLine++;

                if (currentLine < startLine) continue;

                if (currentLine > endLine) break;

                if (string.IsNullOrWhiteSpace(line)) continue;

                if (!line.StartsWith("20")) continue;

                yield return LogItem.FromString(line, currentLine);
            }
        }

        public static IEnumerable<LogItem> ParseLines(string fileName, DateTimeOffset startTime)
        {
            var currentLine = uint.MinValue;

            foreach (var line in File.ReadLines(fileName))
            {
                currentLine++;

                if (string.IsNullOrWhiteSpace(line)) continue;

                if (!line.StartsWith("20")) continue;

                var item = LogItem.FromString(line, currentLine);
                if (item.ProducedAt >= startTime) yield return item;
            }
        }

        public static uint CountLines(string fileName) => Convert.ToUInt32(File.ReadLines(fileName).Count());
    }
}
