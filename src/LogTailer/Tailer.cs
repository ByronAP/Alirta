/*
 * NOT SURE ON THIS!
 * DO WE EVEN NEED TO DO ALL THIS? SHOULD WE JUST PARSE THE LOGS WHEN IT IS TIME TO UPLOAD DATA? PROS/CONS?
 * NOT SURE ON THIS!
 */
using LogTailer.Helpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace LogTailer
{
    public class Tailer : IHostedService
    {
        public event NewLogItem OnNewLogItem;

        private ulong _prevLen = 0ul;

        private readonly int _bufferSize = 4096;
        private readonly string _fileName;
        private readonly uint _initialLines;
        private readonly TimeSpan _pollingInterval;
        private readonly LogLevel _minLogLevel;
#nullable enable
        private readonly ILogger? _logger;
#nullable restore

        public Tailer(string fileName, uint initialLines = 100, uint pollingIntervalMs = 1000, LogLevel minLogLevel = LogLevel.Information)
        {
            if (!File.Exists(fileName)) throw new FileNotFoundException("File does not exist:" + fileName);

            _fileName = fileName;
            _initialLines = initialLines;
            _pollingInterval = TimeSpan.FromMilliseconds(pollingIntervalMs);
            _minLogLevel = minLogLevel;
            _logger = null;
        }

        public Tailer(ITailerOptions options)
        {
            _fileName = options.FileName;
            _initialLines = options.InitialLines;
            _pollingInterval = options.PollingInterval;
            _minLogLevel = options.MinLogLevel;
            _logger = options.Logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var fi = new FileInfo(_fileName);
            _prevLen = Convert.ToUInt64(fi.Length);

            throw new NotImplementedException();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
