using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using AssetStudio;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RediveExtract
{
    static partial class Program
    {
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
    }
}