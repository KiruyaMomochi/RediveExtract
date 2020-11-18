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
    public static class commandsExtension
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

        public static string ToYaml(this IEnumerable<Command> commands)
        {
            serializer ??= new YamlDotNet.Serialization.SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            return serializer.Serialize(commands);
        }

        public static void ToYaml(this IEnumerable<Command> commands, TextWriter writer)
        {
            serializer ??= new YamlDotNet.Serialization.SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            serializer.Serialize(writer, commands);
        }
#nullable disable

        public static string ToJson(this IEnumerable<Command> commands)
        {
            return JsonSerializer.Serialize(commands, _options);
        }

        public static byte[] ToUtf8Json(this IEnumerable<Command> commands)
        {
            return JsonSerializer.SerializeToUtf8Bytes(commands, _options);
        }

        static private ISerializer _serializer = null;
        
        public static string ToReadableYaml(this IEnumerable<Command> commands)
        {
            _serializer ??= new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            var dict = commands.Select(x => x.ToDict()).Where(x => x != null);
            return _serializer.Serialize(dict);
        }
    }
}
