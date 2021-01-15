using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RediveExtract
{
    public class Config
    {
        public int TruthVersion { get; set; } = 14016;
        public string OS { get; set; } = "Android";
        public string Locale { get; set; } = "Jpn";
        public int[] Version { get; set; } = { 2, 3, 0 };

        public static string TruthVersionString(int truthVersion)
        {
            return truthVersion.ToString("D8");
        }

        public static string VersionString(int[] version)
        {
            return $"{version[0]}.{version[1]}.{version[2]}";
        }

        public string ManifestPath(int? truthVersion = null, string locale = null, string os = null)
        {
            truthVersion ??= TruthVersion;
            locale ??= Locale;
            os ??= OS;
            var truthVersionString = TruthVersionString(truthVersion.Value);
            return $"dl/Resources/{truthVersionString}/{locale}/AssetBundles/{os}/";
        }

        public string MoviePath(int? truthVersion = null, string locale = null)
        {
            truthVersion ??= TruthVersion;
            locale ??= Locale;
            var truthVersionString = TruthVersionString(truthVersion.Value);
            return $"dl/Resources/{truthVersionString}/{locale}/Movie/SP/High/";
        }

        public string LowMoviePath(int? truthVersion = null, string locale = null)
        {
            truthVersion ??= TruthVersion;
            locale ??= Locale;
            var truthVersionString = TruthVersionString(truthVersion.Value);
            return $"dl/Resources/{truthVersionString}/{locale}/Movie/SP/Low/";
        }

        public string SoundPath(int? truthVersion = null, string locale = null)
        {
            truthVersion ??= TruthVersion;
            locale ??= Locale;
            var truthVersionString = TruthVersionString(truthVersion.Value);
            return $"dl/Resources/{truthVersionString}/{locale}/Sound/";
        }

        public string BundlesPath(int[] version = null, string locale = null, string os = null)
        {
            version ??= Version;
            locale ??= Locale;
            os ??= OS;

            var versionString = VersionString(version);
            return $"dl/Bundles/{versionString}/{locale}/AssetBundles/{os}/";;
        }
        
    }
}