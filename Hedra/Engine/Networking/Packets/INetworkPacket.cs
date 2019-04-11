namespace Hedra.Engine.Networking.Packets
{
    public interface INetworkPacket<out T>
    {
        MessagePacketType Type { get; }
        T Parse(byte[] Contents);
    }
}