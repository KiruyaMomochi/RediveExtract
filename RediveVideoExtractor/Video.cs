using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VGMToolbox.format;

namespace RediveMediaExtractor
{
    public static class Video
    {
        // ReSharper disable once SuggestBaseTypeForParameter
        public static string[] ExtractUsm(FileInfo file)
        {
            var usm = new CriUsmStream(file.FullName);
            var files = usm.DemultiplexStreams(new MpegStream.DemuxOptionsStruct
            {
                ExtractVideo = true,
                ExtractAudio = true
            });
            return files;
        }

        public static Task M2VToMp4(string input, string output)
            => M2VToMp4(new FileInfo(input), new FileInfo(output));


        public static Task M2VToMp4(string video, IEnumerable<string> audio, string output)
            => M2VToMp4(new FileInfo(video), audio?.Select(x => new FileInfo(x)).ToArray(), new FileInfo(output));

        public static async Task<string> M2VToMp4(string input)
        {
            var ret = Path.ChangeExtension(input, "mp4");
            await M2VToMp4(new FileInfo(input), new FileInfo(ret));
            return ret;
        }

        //ReSharper disable once SuggestBaseTypeForParameter
        public static async Task M2VToMp4(FileInfo input, FileInfo output)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments =
                    $"-hide_banner -loglevel warning -i {input.FullName} " +
                    $"-c copy -map 0 -movflags faststart -y {output.FullName}"
            };
            using var process = new Process {StartInfo = startInfo};
            Console.WriteLine($"{process.StartInfo.FileName} {process.StartInfo.Arguments}");
            process.Start();
            await process.WaitForExitAsync();
        }

        //ReSharper disable once SuggestBaseTypeForParameter
        public static async Task M2VToMp4(FileInfo video, FileInfo[] audio, FileInfo output)
        {
            if (audio == null || audio.Length == 0)
            {
                await M2VToMp4(video, output);
                return;
            }

            var audioStr = string.Join(' ', audio.Select(x => $"-i {x.FullName}"));
            var startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments =
                    $"-hide_banner -loglevel warning -i {video.FullName} {audioStr} " +
                    $"-filter_complex amix=inputs={audio.Length}:duration=longest " +
                    $"-c:v copy -c:a aac -vbr 5 -movflags faststart -y {output.FullName}"
            };
            using var process = new Process {StartInfo = startInfo};
            Console.WriteLine($"{process.StartInfo.FileName} {process.StartInfo.Arguments}");
            process.Start();
            await process.WaitForExitAsync();
        }
    }
}
