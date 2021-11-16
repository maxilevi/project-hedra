namespace Hedra.Engine.Networking.Packets
{
    public class PeersPacket : NetworkPacket<PeersPacket>
    {
        public override MessagePacketType Type => MessagePacketType.PeersPacket;

        public ulong[] PeerIds { get; set; }

        protected override void DoParse(PacketReader Reader, PeersPacket Empty)
        {
            Empty.PeerIds = Reader.ReadArray(Reader.ReadUInt64);
        }

        protected override void DoSerialize(PacketWriter Writer)
        {
            Writer.Write(PeerIds, Writer.Write);
        }
    }
}