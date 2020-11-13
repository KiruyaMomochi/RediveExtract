using RediveStoryDeserializer;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Linq;
using YamlDotNet.Serialization.NamingConventions;

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

        private static void Main(string[] args)
        {
            var rootCommand = new RootCommand("Redive Extractor")
            {
                new Option<FileInfo>("--config"),
                new Option<string>("--output")
            };
            rootCommand.Handler = CommandHandler.Create<FileInfo, string>(Extract);

            var deserialize = new System.CommandLine.Command("deserialize")
            {
                new Option<FileInfo>("--input"),
                new Option<FileInfo>("--json"),
                new Option<FileInfo>("--yaml")
            };
            deserialize.Handler = CommandHandler.Create<FileInfo, FileInfo, FileInfo>(Deserialize);

            rootCommand.Add(deserialize);

            rootCommand.InvokeAsync(args).Wait();
        }

        /// <summary>
        /// Redive Extractor
        /// </summary>
        /// <param name="config">An option whose argument is parsed as a FileInfo. The default value is config.json</param>
        /// <param name="output">The output path</param> 
        private static void Extract(FileInfo config = null, string output = ".")
        {
            _configFile = config ?? new FileInfo("config.json");

            Directory.CreateDirectory(output);
            Directory.SetCurrentDirectory(output);

            Init().Wait();
            SaveAllManifests().Wait();
        }

        private static void Deserialize(FileInfo input = null, FileInfo json = null, FileInfo yaml = null)
        {
            if (input == null)
            {
                Console.WriteLine("input?");
                return;
            }
            if (json == null && yaml == null)
            {
                return;
            }

            var cmds = Deserializer.Deserialize(input);
            if (json != null)
            {
                File.Create(json.FullName).Close();
                using var jsfile = File.Open(json.FullName, FileMode.Truncate);
                jsfile.Write(cmds.ToUtf8Json());
            }
            if (yaml != null)
            {
                var serializer = new YamlDotNet.Serialization.SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                using var sw = new StreamWriter(yaml.FullName, false);
                var dicts = cmds.Select(x => x.ToDict()).Where(x => x != null);
                serializer.Serialize(sw, dicts);
            }
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
