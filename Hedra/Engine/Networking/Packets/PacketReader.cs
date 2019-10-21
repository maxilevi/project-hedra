using System;
using System.IO;
using Hedra.Engine.ItemSystem;
using Hedra.Items;
using System.Numerics;

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

        public ulong ReadUInt64()
        {
            return _reader.ReadUInt64();
        }

        public bool ReadBoolean()
        {
            return _reader.ReadBoolean();
        }
        
        public string ReadString()
        {
            return _reader.ReadString();
        }

        public Vector3 ReadVector3()
        {
            return new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
        }

        public float ReadSingle()
        {
            return _reader.ReadSingle();
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