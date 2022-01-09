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

            var configJsonOption = new Option<FileInfo>("--config", "The config.json file.");
            configJsonOption.AddAlias("-c");

            var input = new Argument<FileInfo>("Input file", "File to be extracted.");

            var outFile = new Option<FileInfo>("--output", "Path to write output file.");
            outFile.AddAlias("-o");

            var outDirectory = new Option<DirectoryInfo>("--outdir",
                () => new DirectoryInfo("."), "Directory to write output.");
            outDirectory.AddAlias("--output");
            outDirectory.AddAlias("-o");

            var outJson = new Option<FileInfo?>("--json", "Path to write json file.");
            var outYaml = new Option<FileInfo?>("--yaml", "Path to write yaml file.");
            var outBinary = new Option<FileInfo?>("--binary", "Path to write binary file.");
            var outLipsync = new Option<FileInfo?>("--lipsync", "Path to write lipsync file.");

            var outVtt = new Option<FileInfo>("--vtt", "Path to write vtt file.");
            outVtt.AddAlias("--output");
            outVtt.AddAlias("-o");

            var imageType = new Option<Unity3d.ImageFormat>("--type", () => Unity3d.ImageFormat.Webp,
                "The image type to extract.");
            imageType.AddAlias("-t");

            var intermediateFiles =
                new Option<bool>("--keep-intermediate", () => false, "Keep intermediate wav files.");
            intermediateFiles.AddAlias("-k");

            var fetch = new Command("fetch", "Fetch latest assets files.")
            {
                configJsonOption, outDirectory,
            };
            fetch.SetHandler((FileInfo config, DirectoryInfo output) => DownloadManifests(config, output.FullName),
                configJsonOption, outDirectory);
            rootCommand.Add(fetch);

            var extract = new Command("extract");

            var database = new Command("database", "Extract database file from unity3d.")
            {
                input, outFile
            };
            database.SetHandler((FileInfo source, FileInfo dest) => Database.ExtractMasterData(source, dest),
                input, outFile);
            extract.Add(database);

            var storyData = new Command("storydata", "Extract story data from unity3d.")
            {
                input, outJson, outYaml, outBinary, outLipsync
            };
            storyData.SetHandler(
                (FileInfo source, FileInfo? json, FileInfo? yaml, FileInfo? binary, FileInfo? lipsync) =>
                    Story.ExtractStoryData(source, json, yaml, binary, lipsync),
                input, outJson, outYaml, outBinary, outLipsync);
            extract.Add(storyData);

            var constText = new Command("consttext", "Extract const text from unity3d.")
            {
                input, outJson, outYaml
            };
            constText.SetHandler((FileInfo source, FileInfo? json, FileInfo? yaml) =>
                    ConstText.ExtractConstText(source, json, yaml),
                input, outJson, outYaml);
            extract.Add(constText);

            var usm = new Command("usm", "Extract videos from usm.")
            {
                input, outDirectory, intermediateFiles
            };
            usm.SetHandler(
                (FileInfo source, DirectoryInfo output, bool keepWav) => Cri.ExtractUsmFinal(source, output, keepWav),
                input, outDirectory, intermediateFiles);
            extract.Add(usm);

            var hca = new Command("hca", "Extract musics from hca.")
            {
                input, outFile
            };
            hca.SetHandler((FileInfo source, FileInfo dest) => Audio.HcaToWav(source, dest),
                input, outFile);
            extract.Add(hca);

            var adx = new Command("adx", "Extract musics from adx.")
            {
                input, outFile
            };
            adx.SetHandler((FileInfo source, FileInfo dest) => Audio.AdxToWav(source, dest),
                input, outFile);
            extract.Add(adx);

            var acb = new Command("acb", "Extract musics from acb.")
            {
                input, outDirectory
            };
            acb.SetHandler((FileInfo source, DirectoryInfo output) => Audio.ExtractAcbCommand(source, output),
                input, outDirectory);
            extract.Add(acb);

            var u3d = new Command("unity3d", "Extract all things in unity3d file.")
            {
                input, outDirectory, imageType
            };

            u3d.SetHandler((FileInfo source, DirectoryInfo output, Unity3d.ImageFormat type) =>
                    Unity3d.ExtractUnity3dCommand(source, output, type),
                input, outDirectory, imageType);

            extract.Add(u3d);

            // TODO: do extract vtt according to MonoBehaviour
            var vtt = new Command("vtt", "Extract vtt from unity3d asset.")
            {
                input, outVtt
            };

            // vtt.Handler =
            //     CommandHandler.Create<FileInfo, FileInfo>(Vtt.ExtractVtt);
            vtt.SetHandler((FileInfo source, FileInfo dest) => Vtt.ExtractVtt(source, dest),
                input, outVtt);
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