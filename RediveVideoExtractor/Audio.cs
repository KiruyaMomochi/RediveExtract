using System;
using System.IO;
using System.Linq;
using DereTore.Exchange.Archive.ACB;
using DereTore.Exchange.Audio.HCA;
using VGAudio.Codecs.CriAdx;
using VGAudio.Codecs.CriHca;
using VGAudio.Containers.Adx;
using VGAudio.Containers.Wave;
using HcaReader = VGAudio.Containers.Hca.HcaReader;

namespace RediveMediaExtractor
{
    public static class Audio
    {
        public static void HcaToWav(FileInfo input, FileInfo output)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (output == null) throw new ArgumentNullException(nameof(output));
            
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

        public static void HcaToWav(Stream input, Stream output, DecodeParams decodeParams)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (output == null) throw new ArgumentNullException(nameof(output));
            
            using var hcaStream = new OneWayHcaAudioStream(input, decodeParams, true);
            hcaStream.CopyTo(output);
        }

        // ReSharper disable once IdentifierTypo
        public static void AcbToWavs(FileInfo source, DirectoryInfo dest)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (dest == null) throw new ArgumentNullException(nameof(dest));
            
            const uint newEncryptionVersion = 0x01300000;

            var acb = AcbFile.FromFile(source.FullName);
            
            var acbFormatVersion = acb.FormatVersion;

            if (acb.ExternalAwb != null)
            {
                var awb = acb.ExternalAwb;
                var decodeParams = DecodeParams.CreateDefault(
                    0x0030D9E8, 0,
                    acbFormatVersion >= newEncryptionVersion ? awb.HcaKeyModifier : 0);

                foreach (var entry in awb.Files)
                {
                    var record = entry.Value;
                    var extractFileName = Path.Combine(dest.FullName,
                        Path.GetFileNameWithoutExtension(source.Name) + $"_{record.CueId:D3}.wav");
                    AfsToWavs(record, File.OpenRead(awb.FileName), decodeParams, extractFileName);
                }
            }
            
            if (acb.InternalAwb != null)
            {
                var awb = acb.InternalAwb;
                var decodeParams = DecodeParams.CreateDefault(
                    0x0030D9E8, 0,
                    acbFormatVersion >= newEncryptionVersion ? awb.HcaKeyModifier : 0);
                
                foreach (var entry in awb.Files)
                {
                    var record = entry.Value;
                    var extractFileName = Path.Combine(dest.FullName,
                        Path.GetFileNameWithoutExtension(source.Name) + $"_{record.CueId:D3}.wav");
                    AfsToWavs(record, acb.Stream, decodeParams, extractFileName);
                }
            }
        }

        // ReSharper disable once IdentifierTypo
        private static void AfsToWavs(Afs2FileRecord afsRecord, Stream awbStream, DecodeParams decodeParams,
            string output)
        {
            using var fileData =
                AcbHelper.ExtractToNewStream(awbStream, afsRecord.FileOffsetAligned, (int) afsRecord.FileLength);
            var isHcaStream = DereTore.Exchange.Audio.HCA.HcaReader.IsHcaStream(fileData);

            if (!isHcaStream)
                return;

            using var fs = File.OpenWrite(output);
            try
            {
                HcaToWav(fileData, fs, decodeParams);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Failed to convert {output}:");
                Console.Error.WriteLine(e);
            }
        }

        public static void AdxToWav(FileInfo input, FileInfo output)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            if (output == null) throw new ArgumentNullException(nameof(output));
            
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