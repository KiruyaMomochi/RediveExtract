using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
                var dict = new Dictionary<int, string>();
                list.ForEach(x =>
                {
                    if (x is OrderedDictionary od && od["TextId"] is int id && od["TextString"] is string str)
                    {
                        dict.Add(id, str.Replace("\\n", "\n"));
                    }
                });
                
                using var fy = yaml.CreateText();
                new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build()
                    .Serialize(fy, dict);
            }
        }
    }
}