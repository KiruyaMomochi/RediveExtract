using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using RediveMediaExtractor;

// ReSharper disable StringLiteralTypo

namespace RediveExtract
{
    static partial class Program
    {
        private static HttpClient _httpClient;
        private static readonly List<Task> Tasks = new();
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
            rootCommand.Handler = CommandHandler.Create<FileInfo, string>(GetManifest);

            var extract = new Command("extract");

            var database = new Command("database")
            {
                new Option<FileInfo>("--source"),
                new Option<FileInfo>("--dest")
            };
            database.Handler = CommandHandler.Create<FileInfo, FileInfo>(ExtractMasterData);
            extract.Add(database);

            var storyData = new Command("storydata")
            {
                new Option<FileInfo>("--source"),
                new Option<FileInfo>("--json"),
                new Option<FileInfo>("--yaml"),
                new Option<FileInfo>("--dest"),
                new Option<FileInfo>("--lipsync")
            };
            storyData.Handler =
                CommandHandler.Create<FileInfo, FileInfo, FileInfo, FileInfo, FileInfo>(ExtractStoryData);
            extract.Add(storyData);

            var constText = new Command("consttext")
            {
                new Option<FileInfo>("--source"),
                new Option<FileInfo>("--json"),
                new Option<FileInfo>("--yaml")
            };
            constText.Handler =
                CommandHandler.Create<FileInfo, FileInfo, FileInfo>(ExtractConstText);
            extract.Add(constText);

            var usm = new Command("usm")
            {
                new Option<FileInfo>("--source"),
                new Option<DirectoryInfo>("--dest"),
            };
            usm.Handler =
                CommandHandler.Create<FileInfo, DirectoryInfo>(ExtractUsmFinal);
            extract.Add(usm);

            var hca = new Command("hca")
            {
                new Option<FileInfo>("--source"),
                new Option<FileInfo>("--dest"),
            };
            hca.Handler =
                CommandHandler.Create<FileInfo, FileInfo>(Audio.HcaToWav);
            extract.Add(hca);

            var adx = new Command("adx")
            {
                new Option<FileInfo>("--source"),
                new Option<FileInfo>("--dest"),
            };
            adx.Handler =
                CommandHandler.Create<FileInfo, FileInfo>(Audio.AdxToWav);
            extract.Add(adx);

            rootCommand.Add(extract);
            rootCommand.InvokeAsync(args).Wait();
        }

        private static async Task ExtractUsmFinal(FileInfo source, DirectoryInfo dest)
        {
            dest ??= source.Directory;
            if (dest == null)
            {
                throw new DirectoryNotFoundException();
            }

            var bins = Video.ExtractUsm(source);
            var taskList = new List<Task>();

            foreach (var bin in bins)
            {
                var noExt = Path.GetFileNameWithoutExtension(Path.GetFileName(bin));
                if (noExt == null)
                    throw new FileNotFoundException();

                switch (Path.GetExtension(bin))
                {
                    case ".bin" or ".hca":
                        taskList.Add(Task.Run(() =>
                            Audio.HcaToWav(bin, Path.Combine(dest.FullName, noExt + ".wav"))
                        ));
                        break;
                    case ".adx":
                        taskList.Add(Task.Run(() =>
                            Audio.AdxToWav(bin, Path.Combine(dest.FullName, noExt + ".wav"))
                        ));
                        break;
                    case ".m2v":
                        taskList.Add(Video.M2VToMp4(bin, Path.Combine(dest.FullName, noExt + ".mp4")));
                        break;
                    default:
                        throw new NotSupportedException(bin);
                }
            }

            await Task.WhenAll(taskList);
            GC.Collect(); 
            GC.WaitForPendingFinalizers(); 
            
            foreach (var bin in bins)
            {
                File.Delete(bin);
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