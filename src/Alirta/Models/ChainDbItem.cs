using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Alirta.Models
{
    internal class ChainDbItem
    {
        [Key]
        public string ChainName { get; set; }

        public string AppVersion { get; set; }

        public Status Status { get; set; } = Status.Unknown;

        public ulong LastSubmittedTimestamp { get; set; } = 0ul;

        public ulong LastLogTimestamp { get; set; } = 0ul;

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

        public virtual IEnumerable<PeerDbItem> Peers { get; set; }

    }
}
