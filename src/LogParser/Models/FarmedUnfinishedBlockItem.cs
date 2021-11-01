namespace LogParser.Models
{
    public class FarmedUnfinishedBlockItem
    {
        //EXAMPLE: Farmed unfinished_block 29f1ffb4b1aeb77ea850e7614965dcf9990b1524f6089727fa88e9ace270ffc8, SP: 12, validation time: 0.39061594009399414, cost: 0
        public string Block { get; set; }
        public uint SP { get; set; }
        public double ValidationTime { get; set; }
        public ulong Cost { get; set; }
    }
}
