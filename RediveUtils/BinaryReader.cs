using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace RediveUtils
{
    public class BinaryReader : System.IO.BinaryReader
    {
        public BinaryReader(Stream input) : this(input, Encoding.UTF8, false)
        {

        }

        public BinaryReader(Stream input, Encoding encoding) : this(input, encoding, false)
        {
        }

        public BinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
        }

        public double ReadDoubleBigEndian()
            => BinaryPrimitives.ReadDoubleBigEndian(base.ReadBytes(8));

        public short ReadInt16BigEndian()
            => BinaryPrimitives.ReadInt16BigEndian(base.ReadBytes(2));

        public int ReadInt32BigEndian()
            => BinaryPrimitives.ReadInt32BigEndian(base.ReadBytes(4));

        public long ReadInt64BigEndian()
            => BinaryPrimitives.ReadInt64BigEndian(base.ReadBytes(8));

        public float ReadSingleBigEndian()
            => BinaryPrimitives.ReadSingleBigEndian(base.ReadBytes(4));

        public ushort ReadUInt16BigEndian()
            => BinaryPrimitives.ReadUInt16BigEndian(base.ReadBytes(2));

        public uint ReadUInt32BigEndian()
            => BinaryPrimitives.ReadUInt32BigEndian(base.ReadBytes(4));

        public ulong ReadUInt64BigEndian()
            => BinaryPrimitives.ReadUInt64BigEndian(base.ReadBytes(8));
    }
}
