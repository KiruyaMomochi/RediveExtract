using System;
using System.Collections.Specialized;
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

                    if (ExtractUnity3dAsset(file, savePath))
                        Console.WriteLine(internalPath);
                    else
                    {
                        Console.Error.WriteLine($"Ignored {file?.GetType()}: {internalPath}");
                    }
                }
                catch (Exception e)
                {
                    Console.Error.Write("Exception occurs when processing");
                    Console.Error.WriteLine($"{internalPath} in {source.FullName}:");
                    Console.Error.WriteLine(e);
                }
            }
        }

        private static bool ExtractUnity3dAsset(object file, string savePath, bool changeExtension = false)
        {
            var saveDir = Path.GetDirectoryName(savePath);
            if (saveDir != null) Directory.CreateDirectory(saveDir);

            switch (file)
            {
                case null:
                    return false;
                case Texture2D texture2D when !savePath.EndsWith(".ttf"):
                {
                    if (changeExtension)
                        savePath = Path.ChangeExtension(savePath, "png");
                    var bitmap = texture2D.ConvertToBitmap(true);
                    bitmap.Save(savePath);
                    break;
                }
                case TextAsset textAsset:
                {
                    if (changeExtension)
                        savePath = Path.ChangeExtension(savePath, "bytes");
                    using var f = File.OpenWrite(savePath);
                    f.Write(textAsset.m_Script);
                    break;
                }
                case MonoBehaviour monoBehaviour:
                {
                    if (changeExtension)
                        savePath = Path.ChangeExtension(savePath, "json");
                    using var f = File.OpenWrite(savePath);
                    JsonSerializer.SerializeAsync(f, monoBehaviour.ToType(), Json.Options).Wait();
                    break;
                }
                case Font font:
                {
                    using var f = File.OpenWrite(savePath);
                    f.Write(font.m_FontData);
                    break;
                }
                case GameObject gameObject when savePath.EndsWith(".prefab"):
                {
                    var newPath = Path.ChangeExtension(savePath, null);
                    var someOk = false;
                    var mustAppendPathId = false;
                    
                    foreach (var component in gameObject.m_Components)
                    {
                        var f = gameObject.assetsFile.ObjectsDic[component.m_PathID];
                        var sp = newPath;
                        
                        if (f is not MonoBehaviour || mustAppendPathId)
                            sp = newPath + "_" + component.m_PathID;
                        else
                            mustAppendPathId = true;
                        
                        var res = ExtractUnity3dAsset(f, sp, true);;
                        if (!res) Console.Error.WriteLine($"Ignored {f?.GetType()} :{component.m_PathID}");
                        someOk |= res;
                    }

                    return someOk;
                }
                default:
                    return false;
            }

            return true;
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