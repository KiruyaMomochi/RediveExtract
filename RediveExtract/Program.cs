using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using AssetStudio;
using RediveStoryDeserializer;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Command = System.CommandLine.Command;
using Deserializer = RediveStoryDeserializer.Deserializer;

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

            var deserialize = new Command("deserialize")
            {
                new Option<FileInfo>("--input"),
                new Option<FileInfo>("--json"),
                new Option<FileInfo>("--yaml")
            };
            deserialize.Handler = CommandHandler.Create<FileInfo, FileInfo, FileInfo>(Deserialize);

            var extract = new Command("extract");

            var database = new Command("database")
            {
                new Option<FileInfo>("--source"),
                new Option<FileInfo>("--dest")
            };
            database.Handler = CommandHandler.Create<FileInfo, FileInfo>(ExtractMasterData);
            extract.Add(database);

            var storydata = new Command("storydata")
            {
                new Option<FileInfo>("--source"),
                new Option<FileInfo>("--json"),
                new Option<FileInfo>("--yaml"),
                new Option<FileInfo>("--dest"),
                new Option<FileInfo>("--lipsync")
            };
            storydata.Handler =
                CommandHandler.Create<FileInfo, FileInfo, FileInfo, FileInfo, FileInfo>(ExtractStoryData);
            extract.Add(storydata);

            var constText = new Command("consttext")
            {
                new Option<FileInfo>("--source"),
                new Option<FileInfo>("--json"),
                new Option<FileInfo>("--yaml")
            };
            constText.Handler =
                CommandHandler.Create<FileInfo, FileInfo, FileInfo>(ExtractConstText);
            extract.Add(constText);
            
            rootCommand.Add(deserialize);
            rootCommand.Add(extract);

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

        private static void ExtractMasterData(FileInfo source, FileInfo dest = null)
        {
            var am = new AssetsManager();
            am.LoadFiles(source.FullName);
            var obj = am.assetsFileList[0].Objects[1];

            dest ??= new FileInfo("master.bytes");
            if (obj is TextAsset database)
            {
                using var f = dest.Create();
                f.Write(database.m_Script);
            }
            else
            {
                throw new NotSupportedException("bundle is not AssetBundle");
            }
        }

        private static byte[] ExtractTextAsset(TextAsset textAsset)
        {
            return textAsset.m_Script;
        }

        private static byte[] ExtractTextAsset(object textAssetObject)
        {
            if (textAssetObject is TextAsset textAsset)
                return textAsset.m_Script;

            throw new NotSupportedException("bundle is not TextAsset");
        }

        private static OrderedDictionary ExtractMonoBehaviour(MonoBehaviour monoBehaviour)
        {
            return monoBehaviour.ToType();
        }

        private static OrderedDictionary ExtractMonoBehaviour(object monoBehaviourObject)
        {
            if (monoBehaviourObject is MonoBehaviour monoBehaviour)
                return monoBehaviour.ToType();

            throw new NotSupportedException("bundle is not AssetBundle");
        }

        private static SerializedFile LoadAssetFile(FileInfo file)
        {
            var am = new AssetsManager();
            am.LoadFiles(file.FullName);
            return am.assetsFileList[0];
        }

        private static SerializedFile LoadAssetFile(string file)
        {
            var am = new AssetsManager();
            am.LoadFiles(file);
            return am.assetsFileList[0];
        }

        private static void ExtractStoryData(FileInfo source, FileInfo json = null, FileInfo yaml = null, FileInfo dest = null,
            FileInfo lipsync = null)
        {
            var file = LoadAssetFile(source);
            var text = file.Objects.OfType<TextAsset>().First().m_Script;
            var ls = file.Objects.OfType<MonoBehaviour>().First().ToType();
            List<RediveStoryDeserializer.Command> commands = null;
            
            if (dest != null)
            {
                using var f = dest.Create();
                f.Write(text);
            }

            if (json != null)
            {
                commands = Deserializer.Deserialize(text);
                using var fj = json.Create();
                fj.Write(commands.ToUtf8Json());
            }

            if (yaml != null)
            {
                commands ??= Deserializer.Deserialize(text);
                using var fy = yaml.CreateText();
                fy.Write(commands.ToReadableYaml());
            }

            if (lipsync != null)
            {
                using var fs = lipsync.Create();
                JsonSerializer.SerializeAsync(fs, ls).Wait();
            }
        }
        
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = JavaScriptEncoder.Create
            (
                UnicodeRanges.BasicLatin,
                UnicodeRanges.CjkUnifiedIdeographs,
                UnicodeRanges.CjkSymbolsandPunctuation,
                UnicodeRanges.Katakana,
                UnicodeRanges.HalfwidthandFullwidthForms
            )
        };

        private static void ExtractConstText(FileInfo source, FileInfo json = null, FileInfo yaml = null)
        {
            var file = LoadAssetFile(source);
            var ls = file.Objects.OfType<MonoBehaviour>().First().ToType();
            var data = ls["dataArray"] ?? new object();

            if (json != null)
            {
                using var fs = json.Create();
                JsonSerializer.SerializeAsync(fs, data, Options).Wait();
            }

            if (yaml != null)
            {
                using var fy = yaml.CreateText();
                new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build()
                    .Serialize(fy, data);
            }
        }

        private static void Deserialize(FileInfo input, FileInfo json = null, FileInfo yaml = null)
        {
            if (json == null && yaml == null)
            {
                return;
            }

            var commands = Deserializer.Deserialize(input);
            if (json != null)
            {
                using var fj = json.Create();
                fj.Write(commands.ToUtf8Json());
            }

            if (yaml != null)
            {
                using var fy = yaml.CreateText();
                fy.Write(commands.ToReadableYaml());
            }
        }

        private static async Task Init()
        {
            _config = await GetConfig();

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseAddress)
            };
        }
    }
}