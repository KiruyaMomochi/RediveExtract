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
            try
            {
                await using var fs = File.OpenRead(FileName);
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
            await using var fs = File.OpenWrite(FileName);
            await JsonSerializer.SerializeAsync(fs, _config);
        }
    }
}