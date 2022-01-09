using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using RediveExtract.Resources;
using RediveMediaExtractor;

// ReSharper disable StringLiteralTypo

namespace RediveExtract
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var rootCommand = new RootCommand("Redive Extractor\n" +
                                              "Download and extract assets from game Princess Connect! Re:Dive.");

            var fetch = new Command("fetch", "Fetch latest assets files.")
            {
                new Option<FileInfo>("--config", "The config.json file."),
                new Option<string>("--output", "The directory to put the output files.")
            };
            fetch.Handler = CommandHandler.Create<FileInfo, string>(DownloadManifests);
            rootCommand.Add(fetch);

            var extract = new Command("extract");

            var database = new Command("database", "Extract database file from unity3d.")
            {
                new Option<FileInfo>("--source", "The unity3d asset file."),
                new Option<FileInfo>("--dest", "Path to the output file.")
            };
            database.Handler = CommandHandler.Create<FileInfo, FileInfo>(Database.ExtractMasterData);
            extract.Add(database);

            var storyData = new Command("storydata", "Extract story data from unity3d.")
            {
                new Option<FileInfo>("--source", "The unity3d asset file."),
                new Option<FileInfo?>("--json", "Path to the output json file"),
                new Option<FileInfo?>("--yaml", "Path to the output yaml file"),
                new Option<FileInfo?>("--dest", "Path to the output binary file"),
                new Option<FileInfo?>("--lipsync", "Path to the output lipsync file")
            };
            storyData.Handler =
                CommandHandler.Create<FileInfo, FileInfo, FileInfo, FileInfo, FileInfo>(Story.ExtractStoryData);
            extract.Add(storyData);

            var constText = new Command("consttext", "Extract const text from unity3d.")
            {
                new Option<FileInfo>("--source", "The unity3d asset file."),
                new Option<FileInfo>("--json", "Path to the output json file"),
                new Option<FileInfo>("--yaml", "Path to the output yaml file")
            };
            constText.Handler =
                CommandHandler.Create<FileInfo, FileInfo, FileInfo>(ConstText.ExtractConstText);
            extract.Add(constText);

            var usm = new Command("usm", "Extract videos from usm.")
            {
                new Option<FileInfo>("--source", "The original usm file."),
                new Option<DirectoryInfo>("--dest", "Path to the output directory."),
            };
            usm.Handler =
                CommandHandler.Create<FileInfo, DirectoryInfo>(Cri.ExtractUsmFinal);
            extract.Add(usm);

            var hca = new Command("hca", "Extract musics from hca.")
            {
                new Option<FileInfo>("--source", "The original hca file."),
                new Option<FileInfo>("--dest", "Path to the output file."),
            };
            hca.Handler =
                CommandHandler.Create<FileInfo, FileInfo>(Audio.HcaToWav);
            extract.Add(hca);

            var adx = new Command("adx", "Extract musics from adx.")
            {
                new Option<FileInfo>("--source", "The original adx file."),
                new Option<FileInfo>("--dest", "Path to the output file."),
            };
            adx.Handler =
                CommandHandler.Create<FileInfo, FileInfo>(Audio.AdxToWav);
            extract.Add(adx);

            var acb = new Command("acb", "Extract musics from acb.")
            {
                new Option<FileInfo>("--source", "The original acb file."),
                new Option<DirectoryInfo>("--dest", "Path to the output directory."),
            };
            acb.Handler =
                CommandHandler.Create<FileInfo, DirectoryInfo>(Audio.AcbToWavsCommand);
            extract.Add(acb);

            var u3d = new Command("unity3d", "Extract all things in unity3d file.")
            {
                new Option<FileInfo>("--source", "The original asset file."),
                new Option<DirectoryInfo>("--dest", "Path to the output directory."),
                new Option<Unity3d.ImageType>("--image", "The image type to extract."),
            };

            u3d.Handler =
                CommandHandler.Create<FileInfo, DirectoryInfo, Unity3d.ImageType?>(Unity3d.ExtractUnity3dCommand);

            extract.Add(u3d);

            // TODO: do extract vtt according to MonoBehaviour
            var vtt = new Command("vtt", "Extract vtt from unity3d asset.")
            {
                new Option<FileInfo>("--source", "The original asset file."),
                new Option<FileInfo>("--dest", "Path to the output file.")
            };

            vtt.Handler =
                CommandHandler.Create<FileInfo, FileInfo>(Vtt.ExtractVtt);
            extract.Add(vtt);

            rootCommand.Add(extract);
            rootCommand.InvokeAsync(args).Wait();
        }

        private static async Task DownloadManifests(FileInfo? config = null, string output = ".")
        {
            config ??= new FileInfo("config.json");

            var manifest = new Manifest(config, output);
            await manifest.SaveAllManifests();
            manifest.SaveConfig(config);
        }
    }
}