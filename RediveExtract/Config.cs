using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RediveExtract
{
    public class Config
    {
        public int TruthVersion { get; set; } = 14016;
        public string OS { get; set; } = "Android";
        public string Locale { get; set; } = "Jpn";
        public int[] Version { get; set; } = { 2, 3, 0 };

        [JsonIgnore] private string TruthVersionString => TruthVersion.ToString("D8");
        [JsonIgnore] private string VersionString => $"{Version[0]}.{Version[1]}.{Version[2]}";
        [JsonIgnore] public string ManifestPath => $"dl/Resources/{TruthVersionString}/{Locale}/AssetBundles/{OS}/";
        [JsonIgnore] public string MoviePath => $"dl/Resources/{TruthVersionString}/{Locale}/Movie/SP/High/";
        [JsonIgnore] public string LowMoviePath => $"dl/Resources/{TruthVersionString}/{Locale}/Movie/SP/Low/";
        [JsonIgnore] public string SoundPath => $"dl/Resources/{TruthVersionString}/{Locale}/Sound/";
        [JsonIgnore] public string BundlesPath => $"dl/Bundles/{VersionString}/{Locale}/AssetBundles/{OS}/";
    }

    static partial class Program
    {
        private static async Task<Config> GetConfig()
        {
            var config = new Config();
            try
            {
                await using var fs = _configFile.OpenRead();
                config = await JsonSerializer.DeserializeAsync<Config>(fs);
            }
            catch (Exception)
            {
                Console.WriteLine("! Warning: config.json not work.");
            }

            return config;
        }

        private static async Task SaveConfig()
        {
            await using var fs = _configFile.OpenWrite();
            await JsonSerializer.SerializeAsync(fs, _config);
        }
    }
}