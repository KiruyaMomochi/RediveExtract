using System;
using System.Collections.Specialized;
using System.CommandLine.Help;
using System.IO;
using System.Text.Json;
using AssetStudio;

namespace RediveExtract
{
    public static class Unity3d
    {
        public static void ExtractUnity3d(FileInfo source, DirectoryInfo dest)
        {
            var am = new AssetsManager();
            am.LoadFiles(source.FullName);
            var dic = am.assetsFileList[0].ObjectsDic;
            if (dic[1] is not AssetBundle assetBundle)
                return;

            var container = assetBundle.m_Container;
            foreach (var (internalPath, value) in container)
            {
                try
                {
                    var id = value.asset.m_PathID;
                    var file = dic[id];
                    var savePath = Path.Combine(dest.FullName, internalPath ?? "unknown");
                    var saveDir = Path.GetDirectoryName(savePath) ?? throw new InvalidOperationException();
                    Directory.CreateDirectory(saveDir);

                    switch (file)
                    {
                        case Texture2D texture2D:
                        {
                            var bitmap = texture2D.ConvertToBitmap(true);
                            bitmap.Save(savePath);
                            break;
                        }
                        case TextAsset textAsset:
                        {
                            using var f = File.OpenWrite(savePath);
                            f.Write(textAsset.m_Script);
                            break;
                        }
                        case MonoBehaviour monoBehaviour:
                        {
                            using var f = File.OpenWrite(savePath);
                            monoBehaviour.ToType();
                            JsonSerializer.SerializeAsync(f, monoBehaviour.ToType(), Json.Options).Wait();
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Exception {e} occurs when processing");
                    Console.WriteLine($"{internalPath} in {source.FullName}");
                }
            }
        }
        
        private static byte[] ExtractTextAsset(TextAsset textAsset)
        {
            return textAsset.m_Script;
        }

        private static byte[] ExtractTextAsset(object textAssetObject)
        {
            if (textAssetObject is TextAsset textAsset)
                return textAsset.m_Script;

            throw new NotSupportedException("bundle is not TextAsset");
        }

        private static OrderedDictionary ExtractMonoBehaviour(MonoBehaviour monoBehaviour)
        {
            return monoBehaviour.ToType();
        }

        private static OrderedDictionary ExtractMonoBehaviour(object monoBehaviourObject)
        {
            if (monoBehaviourObject is MonoBehaviour monoBehaviour)
                return monoBehaviour.ToType();

            throw new NotSupportedException("bundle is not AssetBundle");
        }

        public static SerializedFile LoadAssetFile(FileInfo file)
        {
            var am = new AssetsManager();
            am.LoadFiles(file.FullName);
            return am.assetsFileList[0];
        }

        public static SerializedFile LoadAssetFile(string file)
        {
            var am = new AssetsManager();
            am.LoadFiles(file);
            return am.assetsFileList[0];
        }
    }
}