using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using RediveExtract.Resources;
using RediveMediaExtractor;

// ReSharper disable StringLiteralTypo

namespace RediveExtract
{
    internal static class Program
    {
        private static Task<int> Main(string[] args)
        {
            var configJsonOption = new Option<FileInfo?>("--config", "The config.json file.");
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

            var imageType = new Option<Unity3dResource.ImageFormat>("--type", () => Unity3dResource.ImageFormat.Webp,
                "The image type to extract.");
            imageType.AddAlias("-t");

            var intermediateFiles =
                new Option<bool>("--keep-intermediate", () => false, "Keep intermediate wav files.");
            intermediateFiles.AddAlias("-k");

            var stepOption =
                new Option<bool>("--step", () => false, "Only check next version.");
            stepOption.AddAlias("-s");

            var proxyOption = new Option<string?>("--proxy", "Proxy server to use.");

            var resourceVersion = new Command("resver", "Fetch latest resource version.")
            {
                configJsonOption, stepOption, proxyOption
            };
            resourceVersion.SetHandler(
                (FileInfo? config, bool step, string? proxy) => GuessTruthVersion(config, step, proxy),
                configJsonOption, stepOption, proxyOption);
            
            var bundleVersion = new Command("bdlver", "Fetch latest bundle version.")
            {
                configJsonOption, stepOption, proxyOption
            };
            bundleVersion.SetHandler(
                (FileInfo? config, bool step, string? proxy) => GuessBundleVersion(config, step, proxy),
                configJsonOption, stepOption, proxyOption);

            var fetch = new Command("fetch", "Fetch latest assets files.")
            {
                configJsonOption, outDirectory, stepOption, proxyOption, resourceVersion, bundleVersion
            };
            fetch.SetHandler(
                (FileInfo? config, DirectoryInfo output, bool step, string? proxy) =>
                    DownloadManifests(config, output.FullName, step, proxy),
                configJsonOption, outDirectory, stepOption, proxyOption);

            var database = new Command("database", "Extract database file from unity3d.")
            {
                input, outFile
            };
            database.SetHandler((FileInfo source, FileInfo dest) => DatabaseResource.ExtractMasterData(source, dest),
                input, outFile);

            var storyData = new Command("storydata", "Extract story data from unity3d.")
            {
                input, outJson, outYaml, outBinary, outLipsync
            };
            storyData.SetHandler(
                (FileInfo source, FileInfo? json, FileInfo? yaml, FileInfo? binary, FileInfo? lipsync) =>
                    StoryResource.ExtractStoryData(source, json, yaml, binary, lipsync),
                input, outJson, outYaml, outBinary, outLipsync);

            var constText = new Command("consttext", "Extract const text from unity3d.")
            {
                input, outJson, outYaml
            };
            constText.SetHandler((FileInfo source, FileInfo? json, FileInfo? yaml) =>
                    ConstTextResource.ExtractConstText(source, json, yaml),
                input, outJson, outYaml);

            var usm = new Command("usm", "Extract videos from usm.")
            {
                input, outDirectory, intermediateFiles
            };
            usm.SetHandler(
                (FileInfo source, DirectoryInfo output, bool keepWav) =>
                    CriResource.ExtractUsmFinal(source, output, keepWav),
                input, outDirectory, intermediateFiles);

            var hca = new Command("hca", "Extract musics from hca.")
            {
                input, outFile
            };
            hca.SetHandler((FileInfo source, FileInfo dest) => Audio.HcaToWav(source, dest),
                input, outFile);

            var adx = new Command("adx", "Extract musics from adx.")
            {
                input, outFile
            };
            adx.SetHandler((FileInfo source, FileInfo dest) => Audio.AdxToWav(source, dest),
                input, outFile);

            var acb = new Command("acb", "Extract musics from acb.")
            {
                input, outDirectory
            };
            acb.SetHandler((FileInfo source, DirectoryInfo output) => Audio.ExtractAcbCommand(source, output),
                input, outDirectory);

            var u3d = new Command("unity3d", "Extract all things in unity3d file.")
            {
                input, outDirectory, imageType
            };
            u3d.SetHandler((FileInfo source, DirectoryInfo output, Unity3dResource.ImageFormat type) =>
                    Unity3dResource.ExtractUnity3dCommand(source, output, type),
                input, outDirectory, imageType);

            // TODO: do extract vtt according to MonoBehaviour
            var vtt = new Command("vtt", "Extract vtt from unity3d asset.")
            {
                input, outVtt
            };
            vtt.SetHandler((FileInfo source, FileInfo dest) => VttResource.ExtractVtt(source, dest),
                input, outVtt);

            var extract = new Command("extract")
            {
                database,
                storyData,
                constText,
                usm,
                hca,
                adx,
                acb,
                u3d,
                vtt
            };

            var rootCommand = new RootCommand("Redive Extractor\n" +
                                              "Download and extract assets from game Princess Connect! Re:Dive.")
            {
                fetch,
                extract
            };

            return rootCommand.InvokeAsync(args);
        }

        private static async Task DownloadManifests(
            FileInfo? config = null,
            string output = ".",
            bool step = false,
            string? proxy = null)
        {
            config ??= new FileInfo("config.json");

            var manifest = new Manifest(config, output);
            await manifest.SaveAllManifests(step ? 1 : -1);
            manifest.SaveConfig(config);
        }

        private static async Task<int> GuessTruthVersion(
            FileInfo? config = null,
            bool step = false,
            string? proxy = null)
        {
            config ??= new FileInfo("config.json");
            var manifest = new Manifest(config, string.Empty, proxy);

            var (manifestFile, truthVersion) = await manifest.GuessNewTruthVersion(step ? 1 : -1);
            Console.WriteLine(truthVersion);
            return manifestFile == null ? 0 : 1;
        }


        private static async Task<int> GuessBundleVersion(
            FileInfo? config = null,
            bool step = false,
            string? proxy = null)
        {
            config ??= new FileInfo("config.json");
            var manifest = new Manifest(config, string.Empty, proxy);
            
            var (manifestFile, bundleVersion) = await manifest.GuessNewBundleVersion(step ? 1 : -1);
            Console.WriteLine(ManifestConsts.VersionString(bundleVersion));
            return manifestFile == null ? 0 : 1;
        }
    }
}