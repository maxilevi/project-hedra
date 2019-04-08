namespace Hedra.Engine.Networking.Packets
{
    public interface INetworkPacket
    {
        MessagePacketType Type { get; }   
    }
    
    public abstract class NetworkPacket<T> : INetworkPacket where T : new()
    {
        public abstract MessagePacketType Type { get; }
        
        public T Parse(byte[] Contents)
        {
            var reader = new PacketReader(Contents);
            try
            {
                var obj = new T();
                DoParse(reader, obj);
                return obj;
            }
            finally
            {
                reader.Dispose();
            }
        }

        protected abstract void DoParse(PacketReader Reader, T Empty);

        protected abstract void DoSerialize(PacketWriter Writer);
        
        public byte[] Serialize()
        {
            var writer = new PacketWriter();
            try
            {
                DoSerialize(writer);
                return writer.ToArray();
            }
            finally
            {
                writer.Dispose();
            }
        }
    }
}