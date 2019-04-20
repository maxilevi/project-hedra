using System;
using System.IO;

namespace Hedra.Engine.Player
{
    public interface ISerializableHandler
    {
        void Dump(BinaryWriter Writer);
        void Load(BinaryReader Reader);
    }

    public static class SerializableHandlerExtensions
    {
        public static byte[] Serialize(this ISerializableHandler Handler)
        {
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    Handler.Dump(bw);
                }
                return ms.ToArray();
            }
        }

        public static void UnSerialize(this ISerializableHandler Handler, byte[] Data)
        {
            if(Data.Length == 0) return;
            using (var ms = new MemoryStream(Data))
            {
                using (var br = new BinaryReader(ms))
                {
                    Handler.Load(br);
                }
            }
        }
    }
}