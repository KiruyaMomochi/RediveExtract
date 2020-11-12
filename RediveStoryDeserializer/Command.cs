using System.Text.Json.Serialization;
using YamlDotNet.Serialization;

namespace RediveStoryDeserializer
{
    public record Command
    {
        [YamlIgnore]
        [JsonIgnore]
        public CommandConfig CommandConfig { get; init; }

        [JsonPropertyName("command")]
        public string CommandName => CommandConfig?.Name;

        public string[] Args { get; init; }

        public CommandCategory? Category => CommandConfig?.CommandCategory;

        [YamlIgnore]
        public CommandNumber? Number => CommandConfig?.Number;
    }
}
