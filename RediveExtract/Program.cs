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

        /// <summary>
        /// Redive Extractor
        /// </summary>
        /// <param name="config">An option whose argument is parsed as a FileInfo. The default value is config.json</param>
        /// <param name="output">The output path</param> 
        private static async Task Main(FileInfo config = null, string output = ".")
        {
            _configFile = config ?? new FileInfo("config.json");
            
            Directory.CreateDirectory(output);
            Directory.SetCurrentDirectory(output);

            await Init();
            await SaveAllManifests();
        }

        private static async Task Init()
        {
            _config = await GetConfig();

            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri(BaseAddress)
            };
        }
    }
}
