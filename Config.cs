using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace RediveExtract
{
    static partial class Program
    {
        private static async Task<Config> GetConfig()
        {
            var config = new Config();
            await using var fs = File.OpenRead(FileName);
            try
            {
                config = await JsonSerializer.DeserializeAsync<Config>(fs);
            }
            catch (Exception)
            {
                // ignored
            }

            return config;
        }

        private static async Task SaveConfig()
        {
            await using var fs = File.OpenWrite(FileName);
            await JsonSerializer.SerializeAsync(fs, _config);
        }
    }
}