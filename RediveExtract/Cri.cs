using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using RediveMediaExtractor;

namespace RediveExtract
{
    public static class Cri
    {
        public static async Task ExtractUsmFinal(FileInfo source, DirectoryInfo dest)
        {
            dest ??= source.Directory;
            if (dest == null)
                throw new DirectoryNotFoundException();

            var bins = Video.ExtractUsm(source);
            var taskList = new List<Task>();
            // ReSharper disable once InconsistentNaming
            var m2vs = new List<string>();
            // ReSharper disable once IdentifierTypo
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

            await Task.WhenAll(taskList);

            // ReSharper disable once InconsistentNaming
            taskList.AddRange(from m2v in m2vs
                let mp4 = Path.ChangeExtension(source.Name, "mp4")
                select Video.M2VToMp4(m2v, wavs, Path.Combine(dest.FullName, mp4)));
            await Task.WhenAll(taskList);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            foreach (var bin in bins) File.Delete(bin);
        }

    }
}