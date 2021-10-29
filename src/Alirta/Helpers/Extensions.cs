using Alirta.Models;

namespace Alirta.Helpers
{
    internal static class Extensions
    {
        public static string ToNodeAppName(this NodeEntryPoint value)
        {
            switch (value)
            {
                case NodeEntryPoint.Daemon:
                    return "daemon";
                case NodeEntryPoint.Farmer:
                    return "farmer";
                case NodeEntryPoint.Full_Node:
                    return "full_node";
                case NodeEntryPoint.Harvester:
                    return "harvester";
                case NodeEntryPoint.Wallet:
                    return "wallet";
                default:
                    return string.Empty;
            }
        }
    }
}
