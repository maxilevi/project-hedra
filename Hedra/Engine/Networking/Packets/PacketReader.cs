using System;
using System.IO;
using Hedra.Engine.ItemSystem;

namespace Hedra.Engine.Networking.Packets
{
    public class PacketReader : IDisposable
    {
        private readonly MemoryStream _stream;
        private readonly BinaryReader _reader;

        public PacketReader(byte[] Contents)
        {
            _stream = new MemoryStream(Contents);
            _reader = new BinaryReader(_stream);
        }

        public int ReadInt32()
        {
            return _reader.ReadInt32();
        }

        public string ReadString()
        {
            return _reader.ReadString();
        }

        public Item ReadItem()
        {
            return Item.FromArray(ReadByteArray());
        }

        public byte[] ReadByteArray()
        {
            return _reader.ReadBytes(_reader.ReadInt32());
        }

        public T[] ReadArray<T>(Func<T> ReadEach)
        {
            var array = new T[_reader.ReadInt32()];
            for (var i = 0; i < array.Length; ++i)
            {
                array[i] = ReadEach();
            }
            return array;
        }

        public void Dispose()
        {
            _stream.Dispose();
            _reader.Dispose();
        }
    }
}