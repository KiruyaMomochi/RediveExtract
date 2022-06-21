using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using static RediveExtract.ManifestConsts;

namespace RediveExtract
{
    /// <summary>
    /// Manifest manager - Manipulating manifest files.
    /// </summary>
    public class Manifest
    {
        private readonly Config _config;
        private readonly string _dest;
        private readonly int[] _guessArray = { 1, 10, 11, 100, 101, 1000, 1001, 10000, 10001 };
        public const string ImgServer = "https://img-pc.so-net.tw";

        public HttpClient Client { private get; set; }

        /// <summary>
        /// Create a manifest manager reading <paramref name="configFile"/> and save result to <paramref name="dest"/>.
        /// If config file or destination does not exist, create it.
        /// </summary>
        /// <param name="configFile">Path to config file. The file should be in json format.</param>
        /// <param name="dest">Destination directory to save output.</param>
        public Manifest(FileInfo configFile, string dest, string? proxy = null)
        {
            _dest = dest;
            if (dest != string.Empty)
                Directory.CreateDirectory(dest);

            try
            {
                using var fs = configFile.OpenText();
                _config = JsonSerializer.Deserialize<Config>(fs.ReadToEnd()) ?? new Config();
            }
            catch (Exception)
            {
                Console.WriteLine("config.json not found or not work.");
                throw;
            }

            var handler = new HttpClientHandler();
            if (proxy != null)
            {
                handler.Proxy = new WebProxy(proxy);
            }

            Client = new HttpClient(handler)
            {
                BaseAddress = new Uri(ImgServer),
            };
        }

        /// <summary>
        /// Save all manifests we can found.
        /// </summary>
        public async Task SaveAllManifests(int maxTry = -1)
        {
            var manifests = await SaveLatestAssetManifest(maxTry);
            var tasks = new List<Task>
            {
                SaveLatestBundleManifest(maxTry), SaveMovieManifest(), SaveLowMovieManifest(), SaveSoundManifest()
            };

            tasks.AddRange(ManifestItem
                .ParseAll(manifests)
                .Select(assetManifest =>
                    SaveManifestIfNotExist(
                        _config.AssetEndpoint(),
                        assetManifest.Uri,
                        assetManifest.Uri,
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
        /// Save latest asset manifest.
        /// Try to guess latest asset manifest, then save it.
        /// </summary>
        /// <returns>A string containing content of manifest.</returns>
        private async Task<string> SaveLatestAssetManifest(int maxTry = -1)
        {
            var (manifest, truthVersion) = await GuessNewTruthVersion(maxTry);
            manifest ??= await GetAssetManifest(truthVersion);
            
            WriteAssetManifest(manifest);
            _config.TruthVersion = truthVersion;
            return manifest;
        }

        public async Task<(string? manifest, int truthVersion)> GuessNewTruthVersion(int maxTry = -1)
        {
            if (maxTry == -1)
                maxTry = int.MaxValue;

            var guessedVersions = new HashSet<int>();
            var truthVersion = _config.TruthVersion;
            string? manifest = null;

            for (var i = 0; i < maxTry; i++)
            {
                var (newManifest, newTruthVersion) = await GuessNextTruthVersion(truthVersion, guessedVersions);
                if (newManifest == null)
                    break;
                
                (manifest, truthVersion) = (newManifest, newTruthVersion);
            }

            return (manifest, truthVersion);
        }

        private async Task<(string? manifest, int truthVersion)> GuessNextTruthVersion(int truthVersion, ISet<int> guessedVersions)
        {
            Console.WriteLine($":: Guessing asset manifest from {truthVersion}.");
            string? manifest = null;

            foreach (var guessDelta in _guessArray)
            {
                var guessDigits = (int)Math.Pow(10, Math.Floor(Math.Log10(guessDelta)));
                var guessTruthVersion = truthVersion / guessDigits * guessDigits + guessDelta;

                if (guessedVersions.Contains(guessTruthVersion))
                    continue;

                Console.Write($" Guessing TruthVersion: {guessTruthVersion}...");

                try
                {
                    manifest = await GetAssetManifest(guessTruthVersion);
                    Console.WriteLine("Yes!");
                    return (manifest, guessTruthVersion);
                }
                catch (HttpRequestException)
                {
                    Console.WriteLine("No.");
                    guessedVersions.Add(guessTruthVersion);
                }
            }

            return (manifest, truthVersion);
        }

        /// <summary>
        /// Save latest bundle manifest file.
        /// </summary>
        /// <returns>A string containing content of manifest.</returns>
        private async Task<string> SaveLatestBundleManifest(int maxTry = -1)
        {
            var (manifest, version) = await GuessNewBundleVersion(maxTry);
            manifest ??= await GetBundleManifest(version);
            
            WriteBundleManifest(manifest);
            _config.Version = version;
            return manifest;
        }

        public async Task<(string? manifest, int[] version)> GuessNewBundleVersion(int maxTry = -1)
        {
            if (maxTry == -1)
                maxTry = int.MaxValue;

            var guessedVersions = new HashSet<string>();
            var version = _config.Version; // shallow copy
            string? manifest = null;
            
            for (var i = 0; i < maxTry; i++)
            {
                var (newManifest, newVersion) = await GuessNextBundleVersion(version, guessedVersions);
                if (newManifest == null)
                    break;
                
                (manifest, version) = (newManifest, newVersion);
            }

            return (manifest, version);
        }

        private async Task<(string? manifest, int[] version)> GuessNextBundleVersion(
            int[] version,
            ISet<string> guessedVersions)
        {
            var tmpVersion = version.ToArray(); // deep copy
            
            Console.WriteLine($":: Guessing bundle manifest from {VersionString(tmpVersion)}.");
            string? manifest = null;

            for (var i = tmpVersion.Length - 1; i >= 0; i--)
            {
                tmpVersion[i]++;
                if (guessedVersions.Contains(VersionString(tmpVersion)))
                    continue;

                Console.Write($" Guessing version: {VersionString(tmpVersion)}...");

                try
                {
                    manifest = await GetBundleManifest(tmpVersion);
                    Console.WriteLine("Yes!");
                    return (manifest, tmpVersion);
                }
                catch (HttpRequestException)
                {
                    guessedVersions.Add(VersionString(tmpVersion));
                    Console.WriteLine("No.");
                    tmpVersion[i] = 0;
                }
            }

            return (manifest, version);
        }

        private Task<string> GetAssetManifest(int truthVersion)
        {
            var config = _config with { TruthVersion = truthVersion };
            return GetManifest(config.ManifestUri(ManifestFile.Asset));
        }

        private Task SaveAssetManifest() =>
            SaveManifest(ManifestFile.Asset);

        private void WriteAssetManifest(string manifest)
        {
            var path = CombinePath("manifest/manifest_assetmanifest");
            var dir = Path.GetDirectoryName(path);
            if (dir != null) Directory.CreateDirectory(dir);

            File.WriteAllText(path, manifest);
        }

        private Task<string> GetBundleManifest(int[]? version = null)
        {
            var config = _config with { Version = version ?? _config.Version };
            return GetManifest(config.ManifestUri(ManifestFile.Bundle));
        }

        private Task SaveBundleManifest() => SaveManifest(ManifestFile.Bundle);

        private void WriteBundleManifest(string manifest) =>
            File.WriteAllText(CombinePath(BundleManifest), manifest);

        private async Task SaveMovieManifest()
        {
            await SaveManifest(ManifestFile.Movie);
            await SaveManifest(ManifestFile.Movie2);
        }

        private async Task SaveLowMovieManifest()
        {
            await SaveManifest(ManifestFile.LowMovie);
            await SaveManifest(ManifestFile.LowMovie2);
        }

        private async Task SaveSoundManifest()
        {
            await SaveManifest(ManifestFile.Sound);
            await SaveManifest(ManifestFile.Sound2);
        }

        private async Task<string> GetManifest(string requestUri)
        {
            var m = await Client.GetAsync(requestUri);
            m.EnsureSuccessStatusCode();
            var manifests = await m.Content.ReadAsStringAsync();
            return manifests;
        }

        /// <summary>
        /// Save a manifest file  from <paramref name="requestEndpoint"/>/<paramref name="requestPath"/> to
        /// <paramref name="writePath"/>.
        /// </summary>
        /// <param name="requestEndpoint">Remote endpoint to download file from.</param>
        /// <param name="requestPath">Path to download file from.</param>
        /// <param name="writePath">Path to write file.</param>
        private async Task SaveManifest(string requestEndpoint, string requestPath, string writePath)
        {
            var requestUri = requestEndpoint + requestPath;
            Console.WriteLine($"- {requestUri}");
            var m = await Client.GetAsync(requestUri);

            m.EnsureSuccessStatusCode();
            await using var writeStream = File.OpenWrite(writePath);
            await using var manifests = await m.Content.ReadAsStreamAsync();
            await manifests.CopyToAsync(writeStream);
        }

        private Task SaveManifest(ManifestFile manifestFile) =>
            SaveManifest(
                _config.Endpoint(manifestFile),
                ManifestPath(manifestFile),
                ManifestDest(manifestFile)
            );

        /// <summary>
        /// Save a manifest file from <paramref name="requestEndpoint"/>/<paramref name="requestPath"/> to
        /// <paramref name="writePath"/>.
        /// If <paramref name="md5Sum"/> is provided, skip saving if file already exists and checksum matches.
        /// </summary>
        /// <param name="requestEndpoint">Remote endpoint to download file from.</param>
        /// <param name="requestPath">Path to download file from.</param>
        /// <param name="writePath">Path to write file.</param>
        /// <param name="md5Sum">MD5 checksum.</param>
        private async Task SaveManifestIfNotExist(string requestEndpoint, string requestPath, string writePath,
            string md5Sum)
        {
            if (File.Exists(writePath))
            {
                using var md5 = MD5.Create();
                await using var stream = File.OpenRead(writePath);
                var realSum = BitConverter.ToString(await md5.ComputeHashAsync(stream)).Replace("-", "");
                if (string.Equals(realSum, md5Sum, StringComparison.InvariantCultureIgnoreCase))
                    return;
            }

            await SaveManifest(requestEndpoint, requestPath, writePath);
        }

        private Task SaveManifestIfNotExist(ManifestFile manifestFile, string md5Sum)
            => SaveManifestIfNotExist(
                _config.Endpoint(manifestFile),
                ManifestPath(manifestFile),
                ManifestDest(manifestFile),
                md5Sum);

        public void SaveConfig(FileInfo configFile)
        {
            using var fs = configFile.OpenWrite();
            fs.Write(JsonSerializer.SerializeToUtf8Bytes(_config));
        }
    }
}