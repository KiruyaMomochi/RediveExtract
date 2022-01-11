using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using RediveUtils;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RediveStoryDeserializer
{
    public static class CommandsExtension
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

#nullable enable

        public static string ToYaml(this IEnumerable<Command> commands)
        {
            _serializer ??= new YamlDotNet.Serialization.SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            return _serializer.Serialize(commands);
        }

        public static void ToYaml(this IEnumerable<Command> commands, TextWriter writer)
        {
            _serializer ??= new YamlDotNet.Serialization.SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            _serializer.Serialize(writer, commands);
        }
#nullable disable

        public static string ToJson(this IEnumerable<Command> commands)
        {
            return JsonSerializer.Serialize(commands, Options);
        }

        public static byte[] ToUtf8Json(this IEnumerable<Command> commands)
        {
            return JsonSerializer.SerializeToUtf8Bytes(commands, Options);
        }

        static private ISerializer _serializer = null;
        
        public static string ToReadableYaml(this IEnumerable<Command> commands)
        {
            _serializer ??= new SerializerBuilder()
                .WithEventEmitter(next => new LiteralMultilineEmitter(next))
                .Build();
            var dict = commands.Select(x => x.ToDict()).Where(x => x != null);
            return _serializer.Serialize(dict);
        }
    }
}
