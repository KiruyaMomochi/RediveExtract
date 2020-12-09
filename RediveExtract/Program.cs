using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AssetStudio;
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

            var acb = new Command("acb")
            {
                new Option<FileInfo>("--source"),
                new Option<DirectoryInfo>("--dest"),
            };
            acb.Handler =
                CommandHandler.Create<FileInfo, DirectoryInfo>(Audio.AcbToWavs);
            extract.Add(acb);

            var u3d = new Command("unity3d")
            {
                new Option<FileInfo>("--source"),
                new Option<DirectoryInfo>("--dest")
            };
            u3d.Handler =
                CommandHandler.Create<FileInfo, DirectoryInfo>(ExtractUnity3d);
            extract.Add(u3d);

            // TODO: do extract vtt according to MonoBehaviour
            var vtt = new Command("vtt")
            {
                new Option<FileInfo>("--source"),
                new Option<FileInfo>("--dest")
            };
            vtt.Handler =
                CommandHandler.Create<FileInfo, FileInfo>(ExtractVtt);
            extract.Add(vtt);

            rootCommand.Add(extract);
            rootCommand.InvokeAsync(args).Wait();
        }

        private static void ExtractVtt(FileInfo source, FileInfo dest)
        {
            var am = new AssetsManager();
            am.LoadFiles(source.FullName);
            var srt = am.assetsFileList[0].Objects.OfType<MonoBehaviour>().First();

            using var df = dest.CreateText();
            df.WriteLine($"WEBVTT - {srt.m_Name}\n");
            
            if (srt.ToType()["recordList"] is not IEnumerable<object> records)
            {
                throw new TypeLoadException();
            }

            foreach (OrderedDictionary rec in records)
            {
                df.WriteLine($"{ConvertTime(rec["startTime"])} --> {ConvertTime(rec["endTime"])}");
                df.WriteLine($"- {rec["text"]}");
                df.WriteLine();
            }

            static string ConvertTime(object obj)
            {
                if (obj is not float time)
                {
                    throw new ArgumentException(null, nameof(obj));
                }
                
                var seconds = (long) time;
                var ms = (long) ((time - seconds) * 1000);
                var hr = seconds / 3600;
                var mi = seconds % 3600 / 60;
                var se = seconds % 60;
                return $"{hr:D2}:{mi:D2}:{se:D2}.{ms:D3}";
            }
        }
        

        private static void ExtractUnity3d(FileInfo source, DirectoryInfo dest)
        {
            var am = new AssetsManager();
            am.LoadFiles(source.FullName);
            var dic = am.assetsFileList[0].ObjectsDic;
            if (dic[1] is not AssetBundle assetBundle) 
                return;
            
            var container = assetBundle.m_Container;
            foreach (var (internalPath, value) in container)
            {
                try
                {

                    var id = value.asset.m_PathID;
                    var file = dic[id];
                    var savePath = Path.Combine(dest.FullName, internalPath ?? "unknown");
                    var saveDir = Path.GetDirectoryName(savePath) ?? throw new InvalidOperationException();
                    Directory.CreateDirectory(saveDir);

                    switch (file)
                    {
                        case Texture2D texture2D:
                        {
                            var bitmap = texture2D.ConvertToBitmap(true);
                            bitmap.Save(savePath);
                            break;
                        }
                        case TextAsset textAsset:
                        {
                            using var f = File.OpenWrite(savePath);
                            f.Write(textAsset.m_Script);
                            break;
                        }
                        case MonoBehaviour monoBehaviour:
                        {
                            using var f = File.OpenWrite(savePath);
                            monoBehaviour.ToType();
                            JsonSerializer.SerializeAsync(f, monoBehaviour.ToType(), Options).Wait();
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception {e} occurs when processing");
                    Console.WriteLine($"{internalPath} in {source.FullName}");
                }
            }
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
            // ReSharper disable once InconsistentNaming
            var m2vs = new List<string>();
            // ReSharper disable once IdentifierTypo
            var wavs = new List<string>();

            foreach (var bin in bins)
            {
                var noExt = Path.GetFileNameWithoutExtension(Path.GetFileName(bin));
                if (noExt == null)
                    throw new FileNotFoundException();
                var wavPath = Path.Combine(dest.FullName, noExt + ".wav");

                switch (Path.GetExtension(bin))
                {
                    case ".bin" or ".hca":
                        wavs.Add(wavPath);
                        taskList.Add(Task.Run(() =>
                            Audio.HcaToWav(bin, wavPath)
                        ));
                        break;
                    case ".adx":
                        wavs.Add(wavPath);
                        taskList.Add(Task.Run(() =>
                            Audio.AdxToWav(bin, wavPath)
                        ));
                        break;
                    case ".m2v":
                        m2vs.Add(bin);
                        break;
                    default:
                        throw new NotSupportedException(bin);
                }
            }

            await Task.WhenAll(taskList);

            Tasks.Clear();
            // ReSharper disable once InconsistentNaming
            taskList.AddRange(from m2v in m2vs
                let mp4 = Path.ChangeExtension(source.Name, "mp4")
                select Video.M2VToMp4(m2v, wavs, Path.Combine(dest.FullName, mp4)));
            await Task.WhenAll(taskList);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            foreach (var bin in bins) File.Delete(bin);
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