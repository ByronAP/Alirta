using System;
using System.ComponentModel.DataAnnotations;

namespace Alirta.Models
{
    public class ChainDataItem
    {
        [Key]
        public string ChainName { get; set; }

        public Status Status { get; set; } = Status.Unknown;

        public ulong LastSubmittedTimestamp { get; set; } = Convert.ToUInt64(DateTimeOffset.MinValue.ToUnixTimeMilliseconds());

        public ulong LastLogTimestamp { get; set; } = Convert.ToUInt64(DateTimeOffset.MinValue.ToUnixTimeMilliseconds());

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

    }
}
