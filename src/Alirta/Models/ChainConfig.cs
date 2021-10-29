using Alirta.Contracts;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alirta.Models
{
    internal class ChainConfig : IChainConfig
    {
        [JsonPropertyName("chainName")]
        public string ChainName { get; set; } = "chia";

        [JsonPropertyName("majorCurrencyName")]
        public string MajorCurrencyName { get; set; } = "XCH";

        [JsonPropertyName("minorCurrencyName")]
        public string MinorCurrencyName { get; set; } = "mojo";

        [JsonPropertyName("currencyPrecision")]
        public uint CurrencyPrecision { get; set; } = 12;

        [JsonPropertyName("executableName")]
        public string ExecutableName { get; set; } = "chia";

        [JsonPropertyName("chainFolder")]
        public string ChainFolder { get; set; } = ".chia";

        [JsonPropertyName("appFolder")]
        public string AppFolder { get; set; } = "chia-blockchain";

        [JsonPropertyName("network")]
        public string Network { get; set; } = "mainnet";

        [JsonPropertyName("farmerPort")]
        public uint FarmerPort { get; set; } = 8559;

        [JsonPropertyName("fullNodePort")]
        public uint FullNodePort { get; set; } = 8555;

        [JsonPropertyName("harvesterPort")]
        public uint HarvesterPort { get; set; } = 8560;

        [JsonPropertyName("walletPort")]
        public uint WalletPort { get; set; } = 9256;

        internal static ChainConfig FromJson(string json)
        {
            return JsonSerializer.Deserialize<ChainConfig>(json);
        }
    }
}
