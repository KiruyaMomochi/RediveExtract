using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

// ReSharper disable StringLiteralTypo

namespace RediveExtract
{
    static partial class Program
    {
        private static HttpClient _httpClient;
        private static readonly List<Task> Tasks = new List<Task>();
        private const string BaseAddress = "https://img-pc.so-net.tw";
        private static FileInfo _configFile;
        private static Config _config;
        private static string _outdir;

        /// <summary>
        /// Redive Extractor
        /// </summary>
        /// <param name="config">An option whose argument is parsed as a FileInfo. The default value is config.json</param>
        /// <param name="outdir">Output path</param>
        private static async Task Main(FileInfo config = null, string outdir = ".")
        {
            _configFile = config ?? new FileInfo("config.json");
            _outdir = outdir;

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
            if (!Directory.Exists(_outdir))
            {
                Directory.CreateDirectory(_outdir);
            }
            if (!Directory.Exists(Path.Join(_outdir, "manifest")))
            {
                Directory.CreateDirectory(Path.Join(_outdir, "manifest"));
            }
        }
    }
}
