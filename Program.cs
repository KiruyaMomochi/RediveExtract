using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.IO;
using System.Text.Json.Serialization;

namespace RediveManifest
{
    public struct ManifestItem
    {
        public string Uri;
        public string Md5;
        public string Category;
        public int Length;

        public ManifestItem(string text)
        {
            var fields = text.Split(',');
            if (fields.Length != 5)
                throw new NotImplementedException();
            (Uri, Md5, Category) = (fields[0], fields[1], fields[2]);
            Length = int.Parse(fields[3]);
        }

        static public ManifestItem Parse(string text) => new ManifestItem(text);
    }

    public class Config
    {
        public int TruthVersion { get; set; } = 14016;
        public string OS { get; set; } = "Android";
        public string Locale { get; set; } = "Jpn";
        public int[] Version { get; set; } = { 2, 2, 0 };

        [JsonIgnore]
        public string TruthVersionString => TruthVersion.ToString("D8");
        [JsonIgnore]
        public string VersionString => $"{Version[0]}.{Version[1]}.{Version[2]}";
        [JsonIgnore]
        public string ManifestPath => $"dl/Resources/{TruthVersionString}/{Locale}/AssetBundles/{OS}/";
        [JsonIgnore]
        public string MoviePath => $"dl/Resources/{TruthVersionString}/{Locale}/Movie/SP/High/";
        [JsonIgnore]
        public string LowMoviePath => $"dl/Resources/{TruthVersionString}/{Locale}/Movie/SP/Low/";
        [JsonIgnore]
        public string SoundPath => $"dl/Resources/{TruthVersionString}/{Locale}/Sound/";
        [JsonIgnore]
        public string BundlesPath => $"dl/Bundles/{VersionString}/{Locale}/AssetBundles/{OS}/";
    }

    class Program
    {
        private static HttpClient HttpClient;
        private static readonly List<Task> tasks = new List<Task>();
        private static string fileName = "config.json";

        private static async Task<String> GetManifest(string requestUri, string writePath = null)
        {
            var m = await HttpClient.GetAsync(requestUri);
            m.EnsureSuccessStatusCode();
            var manifests = await m.Content.ReadAsStringAsync();

            if (writePath != null)
                tasks.Add(System.IO.File.WriteAllTextAsync(writePath, manifests));

            return manifests;
        }

        private static ParallelQuery<ManifestItem> ParseAssestManifest(string manifests)
        {
            return manifests.Split().AsParallel().Where(x => x != "").Select(x => ManifestItem.Parse(x));
        }

        private static async Task SaveManifest(string requestUri, string writePath)
        {
            Console.WriteLine(requestUri);
            var m = await HttpClient.GetAsync(requestUri);
            m.EnsureSuccessStatusCode();
            var manifests = await m.Content.ReadAsStringAsync();
            await System.IO.File.WriteAllTextAsync(writePath, manifests);
        }

        static async Task Main(string[] args)
        {
            EnsureDirectory();

            var config = new Config();
            try
            {
                using FileStream fs = File.OpenRead(fileName);
                config = await JsonSerializer.DeserializeAsync<Config>(fs);
            }
            catch (FileNotFoundException) { }

            HttpClient = new HttpClient()
            {
                BaseAddress = new Uri("https://img-pc.so-net.tw")
            };

            await GuessNewVersion(config);
            using (FileStream fs = File.OpenWrite(fileName))
            {
                tasks.Add(JsonSerializer.SerializeAsync(fs, config));
            }

            var manifests = await GetManifest(config.ManifestPath + "manifest/manifest_assetmanifest", "manifest/manifest_assetmanifest");
            tasks.Add(SaveManifest(config.BundlesPath + "manifest/bdl_assetmanifest", "manifest/bdl_assetmanifest"));
            tasks.Add(SaveManifest(config.MoviePath + "manifest/moviemanifest", "manifest/moviemanifest"));
            tasks.Add(SaveManifest(config.LowMoviePath + "manifest/moviemanifest", "manifest/low_moviemanifest"));
            tasks.Add(SaveManifest(config.SoundPath + "manifest/sound2manifest", "manifest/sound2manifest"));

            foreach (var assest_manifest in ParseAssestManifest(manifests))
            {
                var name = assest_manifest.Uri.Split("/")[1];
                tasks.Add(SaveManifest(config.ManifestPath + assest_manifest.Uri, assest_manifest.Uri));
            }

            Task.WaitAll(tasks.ToArray());
        }

        private static void EnsureDirectory()
        {
            if (!Directory.Exists("manifist"))
            {
                Directory.CreateDirectory("manifest");
            }
        }

        private static async Task GuessNewVersion(Config config)
        {
            try
            {
                while (true)
                {
                    config.TruthVersion++;
                    await GetManifest(config.ManifestPath + "manifest/manifest_assetmanifest", "manifest/manifest_assetmanifest");
                }
            }
            catch (HttpRequestException)
            {
                config.TruthVersion--;
            }

            try
            {
                while (true)
                {
                    config.Version[0]++;
                    await GetManifest(config.BundlesPath + "manifest/bdl_assetmanifest", "manifest/bdl_assetmanifest");
                    config.Version[1] = config.Version[2] = 0;
                }
            }
            catch (HttpRequestException)
            {
                config.Version[0]--;
            }

            try
            {
                while (true)
                {
                    config.Version[1]++;
                    await GetManifest(config.BundlesPath + "manifest/bdl_assetmanifest", "manifest/bdl_assetmanifest");
                    config.Version[2] = 0;
                }
            }
            catch (HttpRequestException)
            {
                config.Version[1]--;
            }

            try
            {
                while (true)
                {
                    config.Version[2]++;
                    await GetManifest(config.BundlesPath + "manifest/bdl_assetmanifest", "manifest/bdl_assetmanifest");
                }
            }
            catch (HttpRequestException)
            {
                config.Version[2]--;
            }
        }
    }
}
