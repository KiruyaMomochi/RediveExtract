using System;

namespace RediveExtract;

public static class ManifestConsts
{
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
    public static string MovieEndpoint(int truthVersion, string locale)
    {
        var truthVersionString = TruthVersionString(truthVersion);
        return $"dl/Resources/{truthVersionString}/{locale}/Movie/SP/High/";
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
    public static string LowMovieEndpoint(int truthVersion, string locale)
    {
        var truthVersionString = TruthVersionString(truthVersion);
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
    public static string SoundEndpoint(int truthVersion, string locale)
    {
        var truthVersionString = TruthVersionString(truthVersion);
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
    public static string BundleEndpoint(int[] version, string locale, string os)
    {
        var versionString = VersionString(version);
        return $"dl/Bundles/{versionString}/{locale}/AssetBundles/{os}/";
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
    public static string ManifestsEndpoint(int truthVersion, string locale, string os)
    {
        var truthVersionString = TruthVersionString(truthVersion);
        return $"/dl/Resources/{truthVersionString}/{locale}/AssetBundles/{os}/";
    }

    public enum ManifestFile
    {
        Bundle,
        Movie,
        Movie2,
        LowMovie,
        LowMovie2,
        Sound,
        Sound2,
        Asset
    }
    
    public const string BundleManifest = "manifest/bdl_assetmanifest";
    public const string MovieManifest = "manifest/moviemanifest";
    public const string Movie2Manifest = "manifest/movie2manifest";
    public const string LowMovieManifest = "manifest/moviemanifest";
    public const string LowMovie2Manifest = "manifest/movie2manifest";
    public const string SoundManifest = "manifest/soundmanifest";
    public const string Sound2Manifest = "manifest/sound2manifest";
    public const string AssetManifest = "manifest/manifest_assetmanifest";
    
    public const string LowMovieDest = "manifest/low_moviemanifest";
    public const string LowMovie2Dest = "manifest/low_movie2manifest";

    public static string TruthVersionString(this Config c) => ManifestConsts.TruthVersionString(c.TruthVersion);
    public static string VersionString(this Config c) => ManifestConsts.VersionString(c.Version);
    public static string MovieEndpoint(this Config c) => ManifestConsts.MovieEndpoint(c.TruthVersion, c.Locale);
    public static string LowMovieEndpoint(this Config c) => ManifestConsts.LowMovieEndpoint(c.TruthVersion, c.Locale);
    public static string SoundEndpoint(this Config c) => ManifestConsts.SoundEndpoint(c.TruthVersion, c.Locale);
    public static string BundleEndpoint(this Config c) => ManifestConsts.BundleEndpoint(c.Version, c.Locale, c.OS);
    public static string AssetEndpoint(this Config c) => ManifestConsts.ManifestsEndpoint(c.TruthVersion, c.Locale, c.OS);
    
    public static string Endpoint(this Config c, ManifestFile manifest) => manifest switch
    {
        ManifestFile.Bundle => c.BundleEndpoint(),
        ManifestFile.Movie => c.MovieEndpoint(),
        ManifestFile.Movie2 => c.MovieEndpoint(),
        ManifestFile.LowMovie => c.LowMovieEndpoint(),
        ManifestFile.LowMovie2 => c.LowMovieEndpoint(),
        ManifestFile.Sound => c.SoundEndpoint(),
        ManifestFile.Sound2 => c.SoundEndpoint(),
        ManifestFile.Asset => c.AssetEndpoint(),
        _ => throw new ArgumentException($"Unknown manifest type: {manifest}")
    };
    
    public static string ManifestPath(ManifestFile manifest) => manifest switch
    {
        ManifestFile.Bundle => BundleManifest,
        ManifestFile.Movie => MovieManifest,
        ManifestFile.Movie2 => Movie2Manifest,
        ManifestFile.LowMovie => LowMovieManifest,
        ManifestFile.LowMovie2 => LowMovie2Manifest,
        ManifestFile.Sound => SoundManifest,
        ManifestFile.Sound2 => Sound2Manifest,
        ManifestFile.Asset => AssetManifest,
        _ => throw new ArgumentException($"Unknown manifest type: {manifest}")
    };

    public static string ManifestDest(ManifestFile manifest) => manifest switch
    {
        ManifestFile.LowMovie => LowMovieDest,
        ManifestFile.LowMovie2 => LowMovie2Dest,
        _ => ManifestPath(manifest)
    };

    public static string ManifestUri(this Config c, ManifestFile manifest) 
        => c.Endpoint(manifest) + ManifestPath(manifest);
}
