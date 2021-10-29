using SysInfo.Contracts;
using System;

namespace SysInfo.Hals
{
    internal class WindowsHal : IHal
    {
        public ulong GetMemTotalBytes()
        {
            throw new NotImplementedException();
        }

        public ulong GetMemUsedBytes()
        {
            throw new NotImplementedException();
        }

        public uint GetProcCount()
        {
            throw new NotImplementedException();
        }

        public double GetProcUsagePercentTotal()
        {
            throw new NotImplementedException();
        }
    }
}
