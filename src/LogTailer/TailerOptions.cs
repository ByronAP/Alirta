using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace LogTailer
{
#nullable enable
    public interface ITailerOptions
    {
        string FileName { get; }

        uint InitialLines { get; }

        TimeSpan PollingInterval { get; }

        LogLevel MinLogLevel { get; }

        ILogger? Logger { get; }
    }

    public class TailerOptions : ITailerOptions
    {
        public string FileName { get; }

        public uint InitialLines { get; }

        public TimeSpan PollingInterval { get; }

        public LogLevel MinLogLevel { get; }

        public ILogger? Logger { get; }

        public TailerOptions(string fileName, uint initialLines = 100, uint pollingIntervalMs = 1000, LogLevel minLogLevel = LogLevel.Information, ILogger? logger = null)
        {
            if (!File.Exists(fileName)) throw new FileNotFoundException("File does not exist:" + fileName);

            FileName = fileName;
            InitialLines = initialLines;
            PollingInterval = TimeSpan.FromMilliseconds(pollingIntervalMs);
            MinLogLevel = minLogLevel;
            Logger = logger;
        }
    }
#nullable restore
}
