using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable StringLiteralTypo

namespace RediveExtract
{
    public struct ManifestItem
    {
        public readonly string Uri;
        public readonly string Md5;
        public string Category;
        public int Length;

        private ManifestItem(string text)
        {
            var fields = text.Split(',');
            if (fields.Length != 5)
                throw new NotImplementedException();
            (Uri, Md5, Category) = (fields[0], fields[1], fields[2]);
            Length = int.Parse(fields[3]);
        }

        public static ManifestItem Parse(string text) => new ManifestItem(text);

        public static IEnumerable<ManifestItem> ParseAll(string manifests) =>
            manifests.Split().AsParallel().Where(x => x != "").Select(ManifestItem.Parse);
    }
}