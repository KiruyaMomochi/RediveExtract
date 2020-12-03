using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using YamlDotNet.Serialization.NamingConventions;
using RediveUtils;
using BinaryReader = RediveUtils.BinaryReader;

namespace RediveStoryDeserializer
{
    public class Deserializer
    {
        public static List<Command> Deserialize(FileInfo f) => Deserialize(f.OpenRead());

        public static List<Command> Deserialize(byte[] b) => Deserialize(new MemoryStream(b));

        public static List<Command> Deserialize(Stream s)
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

                Command command = ParseCommand(index, argList);

                commandList.Add(command);
            }

            return commandList;
        }

        private static Command ParseCommand(short index, List<string> argList)
        {
            Command command;
            if (index < CommandConfig.List.Length)
            {
                command = new Command
                {
                    CommandConfig = CommandConfig.List[index],
                    Args = argList.ToArray(),
                    Number = index
                };
            }
            else
            {
                Console.WriteLine("Warning: list index out of range");
                command = new Command
                {
                    CommandConfig = null,
                    Args = argList.ToArray(),
                    Number = index
                };
            }

            return command;
        }
    }
}
