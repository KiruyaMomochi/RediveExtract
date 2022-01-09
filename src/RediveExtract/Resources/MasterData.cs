using System;
using System.IO;
using AssetStudio;

namespace RediveExtract
{
    /// <summary>
    /// Master data resource.
    /// Assume the database is in SQLite format. Encrypted database is not supported yet.
    /// </summary>
    internal static class Database
    {
        /// <summary>
        /// Extract master database from source.
        /// </summary>
        /// <param name="source">The asset file containing database.</param>
        /// <param name="dest">Destination to save file. Default to master.bytes.</param>
        /// <exception cref="NotSupportedException">Bundle is not AssetBundle.</exception>
        public static void ExtractMasterData(FileInfo source, FileInfo? dest = null)
        {
            var am = new AssetsManager();
            am.LoadFiles(source.FullName);
            var obj = am.assetsFileList[0].Objects[1];

            dest ??= new FileInfo("master.bytes");
            if (obj is TextAsset database)
            {
                using var f = dest.Create();
                f.Write(database.m_Script);
            }
            else
            {
                throw new NotSupportedException("bundle is not AssetBundle");
            }
        }
    }
}