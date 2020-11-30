using System;
using System.Diagnostics;
using System.IO;
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
                Arguments = $"-hide_banner -loglevel warning -i {input.FullName} -c copy -map 0 -movflags faststart -y {output.FullName}"
            };
            using var process = new Process { StartInfo = startInfo };
            Console.WriteLine($"{process.StartInfo.FileName} {process.StartInfo.Arguments}");
            process.Start();
            await process.WaitForExitAsync();
        }
    }
}

