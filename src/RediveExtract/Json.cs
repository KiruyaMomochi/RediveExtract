using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace RediveExtract
{
    /// <summary>
    /// Common options for json.
    /// </summary>
    public static class Json
    {
        public static readonly JsonSerializerOptions Options = new()
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
            ),
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals
        };
    }
}