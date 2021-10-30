using Alirta.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Alirta.Models
{
    internal class ChainConfig : IChainConfig
    {
        [JsonPropertyName("id")]
        public uint Id { get; set; }

        [JsonPropertyName("chainName")]
        public string ChainName { get; set; } = "chia";

        [JsonPropertyName("instanceDisplayName")]
        public string InstanceDisplayName { get; set; } = "My Chia";

        [JsonPropertyName("currencyCode")]
        public string CurrencyCode { get; set; } = "XCH";

        [JsonPropertyName("minorCurrencyCode")]
        public string MinorCurrencyCode { get; set; } = "mojo";

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

        [JsonPropertyName("monitorAddresses")]
        public List<string> MonitorAddresses { get; set; } = new List<string>();

        [JsonPropertyName("enableWalletMonitoring")]
        public bool EnableWalletMonitoring { get; set; } = true;

        [JsonPropertyName("blockReward")]
        public decimal BlockReward { get; set; } = 2m;

        [JsonPropertyName("blocksPer10Min")]
        public uint BlocksPer10Min { get; set; } = 32;

        [JsonIgnore]
        public string ConfigFilePath { get; set; }

        internal static ChainConfig FromJson(string json)
        {
            return JsonSerializer.Deserialize<ChainConfig>(json);
        }

        public void Save()
        {
            if (string.IsNullOrWhiteSpace(ConfigFilePath)) throw new Exception("Can not update config file, config file path empty.");

            File.WriteAllTextAsync(ConfigFilePath, JsonSerializer.Serialize(this));
        }
    }
}
