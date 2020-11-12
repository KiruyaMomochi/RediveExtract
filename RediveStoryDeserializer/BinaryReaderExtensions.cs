using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RediveStoryDeserializer
{
    static class BinaryReaderExtensions
    {
        public static string ReadRedive(this BinaryReader br)
        {
            var length = br.ReadInt32BigEndian();

            if (length == 0)
            {
                return string.Empty;
            }

            var bytes = br.ReadBytes(length);
            for (int i = 0; i < length; i += 3)
            {
                bytes[i] ^= 0xff;
            }
            var base64 = Encoding.ASCII.GetString(bytes);
            var decodedBin = Convert.FromBase64String(base64);
            var decodedString = Encoding.UTF8.GetString(decodedBin);
            return decodedString.Replace(@"\n", "\n");
        }
    }
}
