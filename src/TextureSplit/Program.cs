using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using AssetStudio;

namespace TextureSplit
{
    [Serializable]
    public record UISpriteData // TypeDefIndex: 6793
    {
        // Fields
        public string name; // 0x10
        public int x; // 0x18
        public int y; // 0x1C
        public int width; // 0x20
        public int height; // 0x24
        public int borderLeft; // 0x28
        public int borderRight; // 0x2C
        public int borderTop; // 0x30
        public int borderBottom; // 0x34
        public int paddingLeft; // 0x38
        public int paddingRight; // 0x3C
        public int paddingTop; // 0x40
        public int paddingBottom; // 0x44

        // Properties
        // public bool hasBorder { get; }
        // public bool hasPadding { get; }

        // Constructors
        // public UISpriteData();
        public UISpriteData(IDictionary orderedDictionary)
        {
            name = (string) orderedDictionary["name"];
            x = (int) orderedDictionary["x"];
            y = (int) orderedDictionary["y"];
            width = (int) orderedDictionary["width"];
            height = (int) orderedDictionary["height"];
            borderLeft = (int) orderedDictionary["borderLeft"];
            borderRight = (int) orderedDictionary["borderRight"];
            borderTop = (int) orderedDictionary["borderTop"];
            borderBottom = (int) orderedDictionary["borderBottom"];
            paddingLeft = (int) orderedDictionary["paddingLeft"];
            paddingRight = (int) orderedDictionary["paddingRight"];
            paddingTop = (int) orderedDictionary["paddingTop"];
            paddingBottom = (int) orderedDictionary["paddingBottom"];
        }

        // Methods
        // public void SetRect(int x, int y, int width, int height);
        // public void SetPadding(int left, int bottom, int right, int top);
        // public void SetBorder(int left, int bottom, int right, int top);
        // public void CopyFrom(UISpriteData sd);
        // public void CopyBorderFrom(UISpriteData sd);
    }

    class Program
    {
        static void Main(string[] args)
        {
            var am = new AssetsManager();
            // ReSharper disable twice StringLiteralTypo
            am.LoadFiles("C:\\Users\\xtyzw\\Downloads\\all_atlascommon.unity3d");
            var dic = am.assetsFileList[0].ObjectsDic;
            if (dic[1] is not AssetBundle assetBundle)
                return;
            foreach (var (internalPath, value) in assetBundle.m_Container)
            {
                var id = value.asset.m_PathID;
                var file = dic[id];

                if (file is not GameObject gameObject || !internalPath.EndsWith("prefab")) continue;
                var monoBehaviourId = gameObject.m_Components[1].m_PathID;
                if (dic[monoBehaviourId] is not MonoBehaviour monoBehaviour) continue;
                var monoDictionary = monoBehaviour.ToType();

                var materialObj = ((OrderedDictionary) monoDictionary["material"])?["m_PathID"];
                if (materialObj is not long materialId) continue;
                if (dic[materialId] is not Material material) continue;
                if (!material.m_SavedProperties.m_TexEnvs[0].Value.m_Texture.TryGet(out Texture2D texture2D)) continue;
                var bitmap = texture2D.ConvertToImage(true);

                var mSprites = monoDictionary["mSprites"];
                if (mSprites is not IEnumerable<object> datum) continue;

                // Console.WriteLine(bitmap.PixelFormat);
                // foreach (OrderedDictionary data in datum)
                // {
                //     var sprite = new UISpriteData(data);
                //
                //     var pixelFormat = bitmap.PixelFormat switch
                //     {
                //         PixelFormat.Format32bppArgb => PixelFormat.Format32bppPArgb,
                //         _ => bitmap.PixelFormat
                //     };
                //
                //     var cropped = bitmap.Clone(new Rectangle(sprite.x, sprite.y, sprite.width, sprite.height),
                //         pixelFormat);
                //     cropped.Save($"C:\\Users\\xtyzw\\Downloads\\test\\{sprite.name}.png");
                // }
            }
        }
    }
}
