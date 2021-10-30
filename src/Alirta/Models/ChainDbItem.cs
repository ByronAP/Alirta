using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Alirta.Models
{
    internal class ChainDbItem
    {
        [Key]
        public uint Id { get; set; }

        public string ChainName { get; set; }

        public string InstanceDisplayName { get; set; }

        public bool IsFullNodeApiResponsive { get; set; }

        public bool IsFarmerApiResponsive { get; set; }

        public bool IsHarvesterApiResponsive { get; set; }

        public bool IsWalletApiResponsive { get; set; }

        public string Network { get; set; }

        public string AppVersion { get; set; }

        public Status Status { get; set; }

        public uint CurrencyPrecision { get; set; }

        public string CurrencyCode { get; set; }

        public string MinorCurrencyCode { get; set; }

        public ulong SyncedBlockHeight { get; set; }

        public decimal TotalNetspace { get; set; }

        public uint Difficulty { get; set; }

        public ulong LastSubmittedTimestamp { get; set; }

        public ulong LastLogTimestamp { get; set; }

        public uint LongestResponseTime { get; set; }

        public uint AvgResponseTime { get; set; }

        public uint ShortestResponseTime { get; set; }

        public uint PlotsCount { get; set; }

        public uint NftPlotsCount { get; set; }

        public ulong PlottedSpace { get; set; }

        public ulong NftlottedSpace { get; set; }

        public uint DrivesCount { get; set; }

        public uint MissedChallenges { get; set; }

        public uint PoolErrors { get; set; }

        public uint SkippedBlocks { get; set; }

        public virtual List<PeerDbItem> Peers { get; set; }

        public virtual List<MonitorAddress> MonitorAddresses { get; set; }
    }
}
