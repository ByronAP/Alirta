using System;
using System.Runtime.InteropServices;

namespace SysInfo
{
    public static class Hal
    {
        public static ulong GetMemTotalBytes()
        {
            throw new NotImplementedException();
        }

        public static ulong GetMemUsedBytes()
        {
            throw new NotImplementedException();
        }

        public static uint GetProcThreadsCount() => Convert.ToUInt32(Environment.ProcessorCount);

        public static string GetOSDescription() => RuntimeInformation.OSDescription;

        public static double GetProcUsagePercentTotal()
        {
            throw new NotImplementedException();
        }

        public static PlatformID GetPlatform() => Environment.OSVersion.Platform;
    }
}
