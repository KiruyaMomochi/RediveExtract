using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;

namespace RediveExtract
{
    /// <summary>
    /// Manifest manager - Manipulating manifest files.
    /// </summary>
    public class Manifest
    {
        private readonly Config _config;
        private readonly HttpClient _client;
        private readonly string _dest;
        private const string ImgServer = "https://img-pc.so-net.tw";

        /// <summary>
        /// Create a manifest manager reading <paramref name="configFile"/> and save result to <paramref name="dest"/>.
        /// If config file or destination does not exist, create it.
        /// </summary>
        /// <param name="configFile">Path to config file. The file should be in json format.</param>
        /// <param name="dest">Destination directory to save output.</param>
        public Manifest(FileInfo configFile, string dest)
        {
            _dest = dest;
            Directory.CreateDirectory(dest);

            try
            {
                using var fs = configFile.OpenText();
                _config = JsonSerializer.Deserialize<Config>(fs.ReadToEnd());
            }
            catch (Exception)
            {
                Console.WriteLine("config.json not found or not work.");
                throw;
            }

            _client = new HttpClient
            {
                BaseAddress = new Uri(ImgServer)
            };
        }

        /// <summary>
        /// Save all manifests we can found.
        /// </summary>
        public async Task SaveAllManifests()
        {
            var manifests = await SaveLatestAssetManifest();
            var tasks = new List<Task>
            {
                SaveLatestBundleManifest(), SaveMovieManifest(), SaveLowMovieManifest(), SaveSoundManifest()
            };

            tasks.AddRange(ManifestItem
                .ParseAll(manifests)
                .Select(assetManifest =>
                    SaveManifest(
                        _config.ManifestPath() + assetManifest.Uri,
                        CombinePath(assetManifest.Uri),
                        assetManifest.Md5)
                )
            );

            Task.WaitAll(tasks.ToArray());
            tasks.Clear();
        }

        /// <summary>
        /// Combine given relative path with output path.
        /// </summary>
        /// <param name="dest">Path relative to output path.</param>
        /// <returns>A path with output directory prepended.</returns>
        private string CombinePath(string dest) => Path.Combine(_dest, dest);

        /// <summary>
        /// Save a manifest file from <paramref name="requestUri"/> to <paramref name="writePath"/>.
        /// If <paramref name="md5Sum"/> is provided, skip saving if file already exists and checksum matches.
        /// </summary>
        /// <param name="requestUri">Remote url to download file from.</param>
        /// <param name="writePath">Path to write file.</param>
        /// <param name="md5Sum">MD5 checksum.</param>
        private async Task SaveManifest(string requestUri, string writePath, string md5Sum)
        {
            if (File.Exists(writePath))
            {
                using var md5 = MD5.Create();
                await using var stream = File.OpenRead(writePath);
                var realSum = BitConverter.ToString(await md5.ComputeHashAsync(stream)).Replace("-", "");
                if (string.Equals(realSum, md5Sum, StringComparison.InvariantCultureIgnoreCase))
                    return;
            }

            await SaveManifest(requestUri, writePath);
        }

        /// <summary>
        /// Save latest asset manifest.
        /// Try to guess latest asset manifest, then save it.
        /// </summary>
        /// <returns>A string containing content of manifest.</returns>
        private async Task<string> SaveLatestAssetManifest()
        {
            var guessArray = new[] { 1000, 100, 10, 1 };
            var guessTruthVersion = _config.TruthVersion;
            var manifest = await GetAssetManifest(guessTruthVersion);
            Console.WriteLine($":: Saving asset manifest from {guessTruthVersion}.");

            foreach (var guessDelta in guessArray)
            {
                int tmpVersion;

                while (true)
                {
                    tmpVersion = (guessTruthVersion / guessDelta + 1) * guessDelta;
                    Console.Write($" Guessing TruthVersion: {tmpVersion}...");

                    try
                    {
                        manifest = await GetAssetManifest(tmpVersion);
                        Console.WriteLine("Yes!");
                        guessTruthVersion = tmpVersion;
                    }
                    catch (HttpRequestException)
                    {
                        Console.WriteLine("No.");
                        break;
                    }
                }

                if (tmpVersion % 10 != 0) continue;

                try
                {
                    manifest = await GetAssetManifest(tmpVersion);
                    Console.WriteLine("Yes!");
                    guessTruthVersion = tmpVersion;
                }
                catch (HttpRequestException)
                {
                    Console.WriteLine("No.");
                }
            }

            SaveAssetManifest(manifest);
            _config.TruthVersion = guessTruthVersion;
            return manifest;
        }

        /// <summary>
        /// Save latest bundle manifest file.
        /// </summary>
        /// <returns>A string containing content of manifest.</returns>
        private async Task<string> SaveLatestBundleManifest()
        {
            var manifest = await GetBundleManifest();
            var guessVersion = _config.Version; // shallow copy
            Console.WriteLine($":: Saving bundle manifest from {Config.VersionString(guessVersion)}.");

            for (var i = 0; i < guessVersion.Length; i++)
            {
                while (true)
                {
                    var tmpVersion = guessVersion.ToArray(); // deep copy
                    tmpVersion[i]++;
                    for (var j = i + 1; j < guessVersion.Length; j++)
                        tmpVersion[j] = 0;

                    Console.Write($" Guessing version: {Config.VersionString(tmpVersion)}...");

                    try
                    {
                        manifest = await GetBundleManifest(tmpVersion);
                        Console.WriteLine("Yes!");
                        guessVersion = tmpVersion;
                    }
                    catch (HttpRequestException)
                    {
                        Console.WriteLine("No.");
                        break;
                    }
                }
            }

            SaveBundleManifest(manifest);
            _config.Version = guessVersion;
            return manifest;
        }

        private Task<string> GetAssetManifest(int? truthVersion = null, string locale = null, string os = null) =>
            GetManifest(
                _config.ManifestPath(truthVersion, locale, os) + "manifest/manifest_assetmanifest");

        private Task SaveAssetManifest(int? truthVersion = null, string locale = null, string os = null) =>
            SaveManifest(
                _config.ManifestPath(truthVersion, locale, os) + "manifest/manifest_assetmanifest",
                CombinePath("manifest/manifest_assetmanifest"));

        private void SaveAssetManifest(string manifest)
        {
            var path = CombinePath("manifest/manifest_assetmanifest");
            var dir = Path.GetDirectoryName(path);
            if (dir != null) Directory.CreateDirectory(dir);

            File.WriteAllText(path, manifest);
        }

        private Task<string> GetBundleManifest(int[] version = null, string locale = null, string os = null) =>
            GetManifest(
                _config.BundlesPath(version, locale, os) + "manifest/bdl_assetmanifest");

        private Task SaveBundleManifest() => SaveManifest(
            _config.BundlesPath() + "manifest/bdl_assetmanifest",
            CombinePath("manifest/bdl_assetmanifest"));

        private void SaveBundleManifest(string manifest) =>
            File.WriteAllText(CombinePath("manifest/bdl_assetmanifest"), manifest);

        private Task SaveMovieManifest() => SaveManifest(
            _config.MoviePath() + "manifest/moviemanifest",
            CombinePath("manifest/moviemanifest"));

        private Task SaveLowMovieManifest() => SaveManifest(
            _config.LowMoviePath() + "manifest/moviemanifest",
            CombinePath("manifest/low_moviemanifest"));

        private Task SaveSoundManifest() => SaveManifest(
            _config.SoundPath() + "manifest/sound2manifest",
            CombinePath("manifest/sound2manifest"));

        private async Task<string> GetManifest(string requestUri)
        {
            var m = await _client.GetAsync(requestUri);
            m.EnsureSuccessStatusCode();
            var manifests = await m.Content.ReadAsStringAsync();
            return manifests;
        }

        private async Task SaveManifest(string requestUri, string writePath)
        {
            Console.WriteLine($"- {requestUri}");
            var m = await _client.GetAsync(requestUri);
            m.EnsureSuccessStatusCode();
            using var writeStream = File.OpenWrite(writePath);
            using var manifests = await m.Content.ReadAsStreamAsync();
            await manifests.CopyToAsync(writeStream);
        }

        public void SaveConfig(FileInfo configFile)
        {
            using var fs = configFile.OpenWrite();
            fs.Write(JsonSerializer.SerializeToUtf8Bytes(_config));
        }
    }
}