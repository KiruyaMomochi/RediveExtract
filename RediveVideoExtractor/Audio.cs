using System;
using System.IO;
using VGAudio.Codecs.CriAdx;
using VGAudio.Codecs.CriHca;
using VGAudio.Containers.Adx;
using VGAudio.Containers.Hca;
using VGAudio.Containers.Wave;

namespace RediveMediaExtractor
{
    public static class Audio
    {
        public static void HcaToWav(FileInfo input, FileInfo output)
        {
            var data = new HcaReader
            {
                Decrypt = true,
                EncryptionKey = new CriHcaKey(0x0030D9E8)
            }.Read(input.OpenRead());
            using var os = output.Open(FileMode.Create, FileAccess.Write, FileShare.Delete);
            new WaveWriter().WriteToStream(data, os);
        }

        public static void HcaToWav(string input, string output) => HcaToWav(new FileInfo(input), new FileInfo(output));

        public static string HcaToWav(string input)
        {
            var ret = Path.ChangeExtension(input, "wav");
            HcaToWav(new FileInfo(input), new FileInfo(ret));
            return ret;
        }

        public static void AdxToWav(FileInfo input, FileInfo output)
        {
            try
            {
                var data = new AdxReader().Read(input.OpenRead());
                using var os = output.Open(FileMode.Create, FileAccess.Write, FileShare.Delete);
                new WaveWriter().WriteToStream(data, os);
            }
            catch (Exception)
            {
                var data = new AdxReader {EncryptionKey = new CriAdxKey(0x0030D9E8)}.Read(input.OpenRead());
                using var os = output.Open(FileMode.Create, FileAccess.Write, FileShare.Delete);
                new WaveWriter().WriteToStream(data, os);
            }
        }

        public static void AdxToWav(string input, string output) => AdxToWav(new FileInfo(input), new FileInfo(output));

        public static string AdxToWav(string input)
        {
            var ret = Path.ChangeExtension(input, "wav");
            AdxToWav(new FileInfo(input), new FileInfo(ret));
            return ret;
        }
    }
}