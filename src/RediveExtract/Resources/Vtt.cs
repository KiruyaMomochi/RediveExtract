using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using AssetStudio;

namespace RediveExtract
{
    public static class Vtt
    {
        public static void ExtractVtt(FileInfo source, FileInfo dest)
        {
            var am = new AssetsManager();
            am.LoadFiles(source.FullName);
            var srt = am.assetsFileList[0].Objects.OfType<MonoBehaviour>().First();

            using var df = new StreamWriter(dest.OpenWrite());
            // ReSharper disable once StringLiteralTypo
            df.WriteLine($"WEBVTT - {srt.m_Name}\n");

            if (srt.ToType()["recordList"] is not IEnumerable<object> records)
            {
                throw new TypeLoadException();
            }

            foreach (OrderedDictionary rec in records)
            {
                df.WriteLine($"{ConvertTime(rec["startTime"] ?? 0f)} --> {ConvertTime(rec["endTime"] ?? 0f)}");
                df.WriteLine(rec["text"]);
                df.WriteLine();
            }
        }

        private static string ConvertTime(object obj)
        {
            if (obj is not float time)
            {
                throw new ArgumentException(null, nameof(obj));
            }

            var seconds = (long) time;
            var ms = (long) ((time - seconds) * 1000);
            var hr = seconds / 3600;
            var mi = seconds % 3600 / 60;
            var se = seconds % 60;
            return $"{hr:D2}:{mi:D2}:{se:D2}.{ms:D3}";
        }
    }
}