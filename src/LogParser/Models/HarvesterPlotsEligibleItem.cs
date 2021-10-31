namespace LogParser.Models
{
    public class HarvesterPlotsEligibleItem
    {
        // EXAMPLE: 15 plots were eligible for farming 6a44fd9fb4... Found 1 proofs. Time: 2.49995 s. Total 5963 plots
        public uint Plots { get; set; }
        public uint PlotsTotal { get; set; }
        public uint Proofs { get; set; }
        public double Time { get; set; }
    }
}
