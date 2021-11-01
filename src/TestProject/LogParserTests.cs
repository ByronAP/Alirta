using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Xunit;

namespace TestProject
{
    public class LogParserTests
    {
        private string[] GetDebugLogFiles()
        {
            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            var logsPath = Path.Combine(desktopPath, "logs");

            var results = new List<string>();

            foreach (var dir in Directory.EnumerateDirectories(logsPath))
            {
                foreach (var file in Directory.EnumerateFiles(dir, "*.log"))
                {
                    results.Add(file);
                }
            }

            return results.ToArray();
        }
        [Fact]
        public void TestLogParserSeek()
        {
            var logFiles = GetDebugLogFiles();

            foreach (var logFile in logFiles)
            {
                var timingStopWatch = new Stopwatch();

                timingStopWatch.Start();

                var result = LogParser.Parser.ParseLines(logFile);

                // force enumeration
                var resultsArray = result.ToArray();

                timingStopWatch.Stop();

                var totalMs = timingStopWatch.ElapsedMilliseconds;

                Assert.NotEmpty(resultsArray);

                Assert.InRange(totalMs, 0, 1000);
            }
        }
    }
}
