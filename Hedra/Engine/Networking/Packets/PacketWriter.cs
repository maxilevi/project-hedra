using System;
using System.IO;
using System.Numerics;
using Hedra.Engine.ItemSystem;

namespace Hedra.Engine.Networking.Packets
{
    public class PacketWriter : IDisposable
    {
        private readonly MemoryStream _stream;
        private readonly BinaryWriter _writer;

        public PacketWriter()
        {
            _stream = new MemoryStream();
            _writer = new BinaryWriter(_stream);
        }

        public void Dispose()
        {
            _stream.Dispose();
            _writer.Dispose();
        }

        public void Write(ulong Value)
        {
            _writer.Write(Value);
        }

        public void Write(int Value)
        {
            _writer.Write(Value);
        }

        public void Write(byte Value)
        {
            _writer.Write(Value);
        }

        public void Write(float Value)
        {
            _writer.Write(Value);
        }

        public void Write(bool Value)
        {
            _writer.Write(Value);
        }

        public void Write(Vector3 Value)
        {
            _writer.Write(Value.X);
            _writer.Write(Value.Y);
            _writer.Write(Value.Z);
        }

        public void Write<T>(T[] Array, Action<T> Each)
        {
            Write(Array.Length);
            for (var i = 0; i < Array.Length; ++i) Each(Array[i]);
        }

        public void Write(Item Value)
        {
            _writer.Write(Value.ToArray());
        }

        public void Write(byte[] Value)
        {
            _writer.Write(Value.Length);
            _writer.Write(Value);
        }

        public void Write(string Value)
        {
            _writer.Write(Value);
        }

        public byte[] ToArray()
        {
            return _stream.ToArray();
        }
    }
}