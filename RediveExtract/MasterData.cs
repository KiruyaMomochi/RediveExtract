using System;
using System.IO;
using AssetStudio;

namespace RediveExtract
{
    static partial class Program
    {
        private static void ExtractMasterData(FileInfo source, FileInfo dest = null)
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