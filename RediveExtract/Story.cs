using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

            try
            {
                var textFile = file.Objects.OfType<TextAsset>().First();
                var textBuffer = textFile.m_Script;
                var textName = textFile.m_Name;

                List<Command> commands = null;

                Console.WriteLine(textName);

                if (dest != null)
                {
                    using var f = dest.Create();
                    f.Write(textBuffer);
                }

                if (json != null)
                {
                    commands = Deserializer.Deserialize(textBuffer);
                    using var fj = json.Create();
                    fj.Write(commands.ToUtf8Json());
                }

                if (yaml != null)
                {
                    commands ??= Deserializer.Deserialize(textBuffer);
                    using var fy = yaml.CreateText();
                    fy.Write(commands.ToReadableYaml());
                }
            }
            catch (InvalidOperationException e)
            {
                Console.Error.WriteLine($"::warning file={source}::No TextAsset found");
                throw;
            }

            if (lipsync == null) return;
            
            try
            {
                var ls = file.Objects.OfType<MonoBehaviour>().First().ToType();
                using var fs = lipsync.Create();
                JsonSerializer.SerializeAsync(fs, ls).Wait();
            }
            catch (InvalidOperationException)
            {
                Console.Error.WriteLine($"No MonoBehaviour in {source}");
                throw;
            }
        }
    }
}