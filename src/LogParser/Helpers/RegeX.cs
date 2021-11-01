using System.Text.RegularExpressions;

namespace LogParser.Helpers
{
    internal static class RegeX
    {
        const string EligiblePlotsPattern = @"^\d+\s+\w+\s\w+\s\w+\s\w+\s\w+\s+";
        const string FarmedUnfinishedPattern = @"^\w+\s+\w+\s+\S+,\s\w+:\s\d+,\s\w+\s\w+:\s\d+.\d+,\s\w+:\s\d+";

        internal static bool IsHarvesterPlotsEligibleItem(string value) => Regex.IsMatch(value, EligiblePlotsPattern);
        internal static bool IsFarmedUnfinishedBlockItem(string value) => Regex.IsMatch(value, FarmedUnfinishedPattern);
    }
}
