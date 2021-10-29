namespace SysInfo.Contracts
{
    internal interface IHal
    {
        uint GetProcCount();

        double GetProcUsagePercentTotal();

        ulong GetMemTotalBytes();

        ulong GetMemUsedBytes();
    }
}
