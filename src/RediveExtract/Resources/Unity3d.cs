using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text.Json;
using AssetStudio;
using SixLabors.ImageSharp;

namespace RediveExtract
{
    public static class Unity3d
    {
        public enum ImageType
        {
            Png,
            Webp
        }

        private static string ImageFormat(ImageType type)
        {
            return type switch
            {
                ImageType.Png => "png",
                ImageType.Webp => "webp",
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
        }

        internal static void ExtractUnity3dCommand(FileInfo source, DirectoryInfo dest,
            ImageType? imageType = null)
        {
            try
            {
                ExtractUnity3d(source, dest, imageType ?? ImageType.Webp);
            }
            catch (Exception e)
            {
                Console.Error.Write("Exception occurs when processing");
                Console.Error.WriteLine($"{source.FullName}");
                Console.Error.WriteLine(e);
            }
        }

        public static List<string> ExtractUnity3d(FileInfo source, DirectoryInfo dest,
            ImageType imageType = ImageType.Webp)
        {
            var res = new List<string>();

            var am = new AssetsManager();
            am.LoadFiles(source.FullName);
            var dic = am.assetsFileList[0].ObjectsDic;
            if (dic[1] is not AssetBundle assetBundle)
                throw new NotSupportedException($"{dic[1].GetType()} is not an AssetBundle.");

            var container = assetBundle.m_Container;
            foreach (var (internalPath, value) in container)
            {
                var id = value.asset.m_PathID;
                var file = dic[id];
                var savePath = Path.Combine(dest.FullName, internalPath ?? "unknown");

                var r = ExtractUnity3dAsset(file, savePath, imageType);
                if (r == null)
                    Console.Error.WriteLine($"Ignored {file?.GetType()}: {internalPath}");
                else
                    res.AddRange(r);
            }

            return res;
        }

        private static List<string>? ExtractUnity3dAsset(object file, string savePath,
            ImageType imageType = ImageType.Webp, bool changeExtension = false)
        {
            var res = new List<string>();

            var saveDir = Path.GetDirectoryName(savePath);
            if (saveDir != null) Directory.CreateDirectory(saveDir);

            switch (file)
            {
                case null:
                    return null;
                case Texture2D texture2D when !savePath.EndsWith(".ttf"):
                {
                    savePath = Path.ChangeExtension(savePath, ImageFormat(imageType));
                    var bitmap = texture2D.ConvertToImage(true);

                    bitmap.Save(savePath);
                    res.Add(savePath);
                    break;
                }
                case TextAsset textAsset:
                {
                    if (changeExtension)
                        savePath = Path.ChangeExtension(savePath, "bytes");
                    using var f = File.OpenWrite(savePath);
                    f.Write(textAsset.m_Script);
                    res.Add(savePath);
                    break;
                }
                case MonoBehaviour monoBehaviour:
                {
                    if (changeExtension)
                        savePath = Path.ChangeExtension(savePath, "json");
                    using var f = File.OpenWrite(savePath);
                    JsonSerializer.SerializeAsync(f, monoBehaviour.ToType(), Json.Options).Wait();
                    res.Add(savePath);
                    break;
                }
                case Font font:
                {
                    using var f = File.OpenWrite(savePath);
                    f.Write(font.m_FontData);
                    res.Add(savePath);
                    break;
                }
                case GameObject gameObject when savePath.EndsWith(".prefab"):
                {
                    var newPath = Path.ChangeExtension(savePath, null);
                    var mustAppendPathId = false;

                    foreach (var component in gameObject.m_Components)
                    {
                        var f = gameObject.assetsFile.ObjectsDic[component.m_PathID];
                        var sp = newPath;

                        if (f is not MonoBehaviour || mustAppendPathId)
                            sp = newPath + "_" + component.m_PathID;
                        else
                            mustAppendPathId = true;

                        var innerRes = ExtractUnity3dAsset(f, sp, imageType, true);
                        ;

                        if (innerRes == null || innerRes.Count == 0)
                            Console.Error.WriteLine($"Ignored {f?.GetType()} :{component.m_PathID}");
                        else res.AddRange(innerRes);
                    }

                    break;
                }
                default:
                    return null;
            }

            return res;
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