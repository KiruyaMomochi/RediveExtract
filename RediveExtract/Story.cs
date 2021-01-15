using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using AssetStudio;
using RediveStoryDeserializer;

namespace RediveExtract
{
    public static class Story
    {
        public static void ExtractStoryData(FileInfo source, FileInfo json = null, FileInfo yaml = null,
            FileInfo dest = null,
            FileInfo lipsync = null)
        {
            var file = Unity3d.LoadAssetFile(source);
            var text = file.Objects.OfType<TextAsset>().First().m_Script;
            var ls = file.Objects.OfType<MonoBehaviour>().First().ToType();
            List<Command> commands = null;

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
    }
}