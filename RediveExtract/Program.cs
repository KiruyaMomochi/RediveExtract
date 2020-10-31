using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

// ReSharper disable StringLiteralTypo

namespace RediveExtract
{
    public struct ManifestItem
    {
        public string Uri;
        public string Md5;
        public string Category;
        public int Length;

        private ManifestItem(string text)
        {
            var fields = text.Split(',');
            if (fields.Length != 5)
                throw new NotImplementedException();
            (Uri, Md5, Category) = (fields[0], fields[1], fields[2]);
            Length = int.Parse(fields[3]);
        }

        public static ManifestItem Parse(string text) => new ManifestItem(text);

        public static IEnumerable<ManifestItem> ParseAll(string manifests) =>
            manifests.Split().AsParallel().Where(x => x != "").Select(ManifestItem.Parse);
    }

    public class Config
    {
        public int TruthVersion { get; set; } = 14016;
        public string OS { get; set; } = "Android";
        public string Locale { get; set; } = "Jpn";
        public int[] Version { get; set; } = {2, 3, 0};

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
        private static HttpClient _httpClient;
        private static readonly List<Task> Tasks = new List<Task>();
        private const string BaseAddress = "https://img-pc.so-net.tw";
        private const string FileName = "config.json";
        private static Config _config;

        private static async Task Main(string[] args)
        {
            await Init();
            await SaveAllManifests();

        }

        private static async Task Init()
        {
            EnsureDirectory();
            _config = await GetConfig();

            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri(BaseAddress)
            };
        }

        private static void EnsureDirectory()
        {
            if (!Directory.Exists("manifist"))
            {
                Directory.CreateDirectory("manifest");
            }
        }
    }
}