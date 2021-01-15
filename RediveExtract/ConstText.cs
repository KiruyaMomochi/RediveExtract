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
    public static class ConstText
    {
        public static void ExtractConstText(FileInfo source, FileInfo json = null, FileInfo yaml = null)
        {
            var file = Unity3d.LoadAssetFile(source);
            var ls = file.Objects.OfType<MonoBehaviour>().First().ToType();
            var data = ls["dataArray"] ?? new object();

            if (json != null)
            {
                using var fs = json.Create();
                JsonSerializer.SerializeAsync(fs, data, Json.Options).Wait();
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