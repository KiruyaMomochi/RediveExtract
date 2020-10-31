using System.Net.Http;
using System.Threading.Tasks;
// ReSharper disable StringLiteralTypo

namespace RediveExtract
{
    static partial class Program
    {
        private static Task<string> SaveAssetManifest() => SaveManifest(
            _config.ManifestPath + "manifest/manifest_assetmanifest",
            "manifest/manifest_assetmanifest");

        private static Task<string> SaveBundleManifest() => SaveManifest(
            _config.BundlesPath + "manifest/bdl_assetmanifest",
            "manifest/bdl_assetmanifest");

        private static Task SaveMovieManifest() => SaveManifest(
            _config.MoviePath + "manifest/moviemanifest",
            "manifest/moviemanifest");
        
        private static Task SaveLowMovieManifest() => SaveManifest(
            _config.LowMoviePath + "manifest/moviemanifest",
            "manifest/low_moviemanifest");

        private static Task SaveSoundManifest() => SaveManifest(
            _config.SoundPath + "manifest/sound2manifest",
            "manifest/sound2manifest");

        private static async Task<string> SaveLatestBundleManifest()
        {
            var manifest = await SaveBundleManifest();

            try
            {
                while (true)
                {
                    _config.Version[0]++;
                    await SaveBundleManifest();
                    _config.Version[1] = _config.Version[2] = 0;
                }
            }
            catch (HttpRequestException)
            {
                _config.Version[0]--;
            }

            try
            {
                while (true)
                {
                    _config.Version[1]++;
                    await SaveBundleManifest();
                    _config.Version[2] = 0;
                }
            }
            catch (HttpRequestException)
            {
                _config.Version[1]--;
            }

            try
            {
                while (true)
                {
                    _config.Version[2]++;
                    await SaveBundleManifest();
                }
            }
            catch (HttpRequestException)
            {
                _config.Version[2]--;
            }

            return manifest;
        }
        
        
        private static async Task<string> SaveLatestAssetManifest()
        {
            var manifest = await SaveAssetManifest();

            try
            {
                while (true)
                {
                    _config.TruthVersion++;
                    manifest = await SaveAssetManifest();
                }
            }
            catch (HttpRequestException)
            {
                _config.TruthVersion--;
            }

            return manifest;
        }

        private static async Task<string> GetManifest(string requestUri)
        {
            var m = await _httpClient.GetAsync(requestUri);
            m.EnsureSuccessStatusCode();
            var manifests = await m.Content.ReadAsStringAsync();
            return manifests;
        }

        private static async Task<string> SaveManifest(string requestUri, string writePath)
        {
            var manifests = await GetManifest(requestUri);
            await System.IO.File.WriteAllTextAsync(writePath, manifests);
            return manifests;
        }
    }
}