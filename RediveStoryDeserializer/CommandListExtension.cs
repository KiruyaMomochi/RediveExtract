using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RediveStoryDeserializer
{
    public static class CommandListExtension
    {
        private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
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

#nullable enable
        private static ISerializer? serializer;

        public static string ToYaml(this IEnumerable<Command> commandList)
        {
            serializer ??= new YamlDotNet.Serialization.SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            return serializer.Serialize(commandList);
        }

        public static void ToYaml(this IEnumerable<Command> commandList, TextWriter writer)
        {
            serializer ??= new YamlDotNet.Serialization.SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            serializer.Serialize(writer, commandList);
        }
#nullable disable

        public static string ToJson(this IEnumerable<Command> commandList)
        {
            return JsonSerializer.Serialize(commandList, _options);
        }

        public static byte[] ToUtf8Json(this IEnumerable<Command> commandList)
        {
            return JsonSerializer.SerializeToUtf8Bytes(commandList, _options);
        }
    }
}
