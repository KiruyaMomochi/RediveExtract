namespace RediveExtract
{
    public class Config
    {
        /// <summary>
        /// Real version number. It's may internally called "resource version". Different versions have different
        /// version format, and updating of version may not continuous.
        /// </summary>
        public int TruthVersion { get; set; } = 14016;
        
        /// <summary>
        /// Operating system. Commonly "Android" or "iOS".
        /// </summary>
        public string OS { get; set; } = "Android";
        
        /// <summary>
        /// Locale of the game. However, in So-net version, it's still "Jpn". Why? 
        /// </summary>
        public string Locale { get; set; } = "Jpn";
        
        /// <summary>
        /// Version number.
        /// </summary>
        public int[] Version { get; set; } = { 2, 3, 0 };

        /// <summary>
        /// Convert truth version to string. In So-net version, padding the version number at left with '0' to 8 digits.
        /// </summary>
        public static string TruthVersionString(int truthVersion)
        {
            return truthVersion.ToString("D8");
        }

        /// <summary>
        /// Convert version to string.
        /// </summary>
        public static string VersionString(int[] version)
        {
            return $"{version[0]}.{version[1]}.{version[2]}";
        }

        /// <summary>
        /// Path to asset bundle manifest url prefix.
        /// Need to combine with other parts to access file.
        /// </summary>
        /// <param name="truthVersion">Truth version string.</param>
        /// <param name="locale">Game locale.</param>
        /// <param name="os">Operating system.</param>
        /// <returns>Absolute URL to manifest path prefix.</returns>
        /// <example>
        /// var config = new Config();
        /// var requestUri = config.ManifestPath(truthVersion, locale, os) + "manifest/manifest_assetmanifest";
        /// </example>
        public string ManifestPath(int? truthVersion = null, string locale = null, string os = null)
        {
            truthVersion ??= TruthVersion;
            locale ??= Locale;
            os ??= OS;
            var truthVersionString = TruthVersionString(truthVersion.Value);
            return $"/dl/Resources/{truthVersionString}/{locale}/AssetBundles/{os}/";
        }

        /// <summary>
        /// Path to high-quality movie manifest url prefix.
        /// Need to combine with other parts to access file.
        /// </summary>
        /// <param name="truthVersion">Truth version string.</param>
        /// <param name="locale">Game locale.</param>
        /// <returns>Absolute URL to manifest path prefix.</returns>
        /// <example>
        /// var config = new Config();
        /// var requestUri = config.MoviePath(truthVersion, locale) + "manifest/moviemanifest";
        /// </example>
        public string MoviePath(int? truthVersion = null, string locale = null)
        {
            truthVersion ??= TruthVersion;
            locale ??= Locale;
            var truthVersionString = TruthVersionString(truthVersion.Value);
            return $"/dl/Resources/{truthVersionString}/{locale}/Movie/SP/High/";
        }

        /// <summary>
        /// Path to low-quality movie manifest url prefix.
        /// Need to combine with other parts to access file.
        /// </summary>
        /// <param name="truthVersion">Truth version string.</param>
        /// <param name="locale">Game locale.</param>
        /// <returns>Absolute URL to manifest path prefix.</returns>
        /// <example>
        /// var config = new Config();
        /// var requestUri = config.LowMoviePath(truthVersion, locale) + "manifest/moviemanifest";
        /// </example>
        public string LowMoviePath(int? truthVersion = null, string locale = null)
        {
            truthVersion ??= TruthVersion;
            locale ??= Locale;
            var truthVersionString = TruthVersionString(truthVersion.Value);
            return $"dl/Resources/{truthVersionString}/{locale}/Movie/SP/Low/";
        }

        /// <summary>
        /// Path to sound manifest url prefix.
        /// Need to combine with other parts to access file.
        /// </summary>
        /// <param name="truthVersion">Truth version string.</param>
        /// <param name="locale">Game locale.</param>
        /// <returns>Absolute URL to manifest path prefix.</returns>
        /// <example>
        /// var config = new Config();
        /// var requestUri = config.SoundPath(truthVersion, locale) + "manifest/sound2manifest";
        /// </example>
        public string SoundPath(int? truthVersion = null, string locale = null)
        {
            truthVersion ??= TruthVersion;
            locale ??= Locale;
            var truthVersionString = TruthVersionString(truthVersion.Value);
            return $"dl/Resources/{truthVersionString}/{locale}/Sound/";
        }

        /// <summary>
        /// Path to bundles url prefix.
        /// Need to combine with other parts to access file.
        /// </summary>
        /// <param name="version">Version number, in format of <c>x.y.z</c>.</param>
        /// <param name="locale">Game locale.</param>
        /// <param name="os">Operating system.</param>
        /// <returns>Absolute URL to manifest path prefix.</returns>
        /// <example>
        /// var config = new Config();
        /// var requestUri = config.BundlesPath(version, locale, os) + "manifest/bdl_assetmanifest“;
        /// </example>
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