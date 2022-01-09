using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RediveMediaExtractor;

namespace RediveExtract
{
    /// <summary>
    /// CriWare video or video resources.
    /// </summary>
    public static class Cri
    {
        public static async Task<List<string>> ExtractUsmFinal(FileInfo source, DirectoryInfo? dest)
        {
            var res = new List<string>();
            dest ??= source.Directory;
            if (dest == null)
                throw new DirectoryNotFoundException();

            var bins = Video.ExtractUsm(source);
            var taskList = new List<Task>();
            var m2vs = new List<string>();
            var wavs = new List<string>();

            foreach (var bin in bins)
            {
                var noExt = Path.GetFileNameWithoutExtension(Path.GetFileName(bin));
                if (noExt == null)
                    throw new FileNotFoundException();
                var wavPath = Path.Combine(dest.FullName, noExt + ".wav");

                switch (Path.GetExtension(bin))
                {
                    case ".bin" or ".hca":
                        wavs.Add(wavPath);
                        taskList.Add(Task.Run(() =>
                            Audio.HcaToWav(bin, wavPath)
                        ));
                        break;
                    case ".adx":
                        wavs.Add(wavPath);
                        taskList.Add(Task.Run(() =>
                            Audio.AdxToWav(bin, wavPath)
                        ));
                        break;
                    case ".m2v":
                        m2vs.Add(bin);
                        break;
                    default:
                        throw new NotSupportedException(bin);
                }
            }

            res.AddRange(wavs);
            await Task.WhenAll(taskList);

            var baseName = Path.Combine(dest.FullName, Path.ChangeExtension(source.Name, null));

            taskList.AddRange(m2vs.Select(
                (m2v, i) =>
                {
                    var mp4 = baseName;
                    if (i == 0)
                        mp4 += ".mp4";
                    else
                        mp4 = $"{mp4}_{i}.mp4";

                    res.Add(mp4);
                    return Video.M2VToMp4(m2v, wavs, mp4);
                })
            );
            await Task.WhenAll(taskList);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            foreach (var bin in bins) File.Delete(bin);

            return res;
        }
    }
}