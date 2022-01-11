using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text.Json;
using AssetStudio;
using RediveExtract.Generated;
using RediveUtils;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.EventEmitters;
using YamlDotNet.Serialization.NamingConventions;

namespace RediveExtract.Resources
{
    /// <summary>
    /// Const text resource.
    /// Priconne saves strings in an object, and reference them in other places, like a embedded database.
    /// </summary>
    public static class ConstTextResource
    {
        public class ConstText
        {
            public string? Name { get; set; }
            public string Value { get; set; } = "";
        }

        /// <summary>
        /// Extract text resource from const text file.
        /// </summary>
        /// <param name="source">The Unity asset file.</param>
        /// <param name="json">Export json to given file. If this is null, no export is done.</param>
        /// <param name="yaml">Export yaml to given file. If this is null, no export is done.</param>
        public static void ExtractConstText(FileInfo source, FileInfo? json = null, FileInfo? yaml = null)
        {
            var file = Unity3dResource.LoadAssetFile(source);
            var ls = file.Objects.OfType<MonoBehaviour>().First().ToType();
            var data = ls["dataArray"];

            if (data is not List<object> list)
            {
                Console.Error.WriteLine($"Invalid dataArray type {data?.GetType()}.");
                return;
            }

            if (json != null)
            {
                using var fs = json.Create();
                JsonSerializer.SerializeAsync(fs, list, Json.Options).Wait();
            }

            if (yaml != null)
            {
                var dict = new Dictionary<int, ConstText>();
                list.ForEach(x =>
                {
                    if (x is OrderedDictionary od && od["TextId"] is int id && od["TextString"] is string str)
                    {
                        str = str.Replace("\\n", "\n");
                        var textId = (TextId)id;
                        dict.Add(id,
                            new ConstText
                                { Name = Enum.IsDefined(textId) ? textId.ToString() : null, Value = str });
                    }
                });

                using var fy = yaml.CreateText();
                var serializer = new SerializerBuilder()
                    .WithEventEmitter(next => new LiteralMultilineEmitter(next))
                    .Build();
                serializer.Serialize(fy, dict);
            }
        }
    }
}