using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using YamlDotNet.Serialization.NamingConventions;

namespace RediveStoryDeserializer
{
    public class Deserializer
    {
        static public List<Command> Deserialize(FileInfo f) => Deserialize(f.OpenRead());

        static public List<Command> Deserialize(Stream s)
        {
            if (!s.CanSeek)
                throw new NotSupportedException("The stream can't be seeked");
            using var br = new BinaryReader(s);
            var commandList = new List<Command>();

            while (true)
            {
                short index;
                try { index = br.ReadInt16BigEndian(); }
                catch (ArgumentOutOfRangeException) { break; }

                var argList = new List<string>();
                for (var st = br.ReadRedive(); st.Length != 0; st = br.ReadRedive())
                {
                    argList.Add(st);
                }

                commandList.Add(
                    new Command
                    {
                        CommandConfig = CommandConfig.List[index],
                        Args = argList.ToArray()
                    });
            }

            return commandList;
        }
    }
}
