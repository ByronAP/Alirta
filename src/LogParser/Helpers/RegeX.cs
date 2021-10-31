using System.Text.RegularExpressions;

namespace LogParser.Helpers
{
    internal static class RegeX
    {
        const string EligiblePlotsPattern = @"^\d+\s+\w+\s\w+\s\w+\s\w+\s\w+\s+";

        internal static bool IsHarvesterPlotsEligibleItem(string value) => Regex.IsMatch(value, EligiblePlotsPattern);
    }
}
