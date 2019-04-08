namespace Hedra.Engine.Networking.Packets
{
    public class WorldPacket : NetworkPacket<WorldPacket>
    {
        public int Seed { get; set; }

        protected override void DoParse(PacketReader Reader, WorldPacket Empty)
        {
            Empty.Seed = Reader.ReadInt32();
        }

        protected override void DoSerialize(PacketWriter Writer)
        {
            Writer.Write(Seed);
        }

        public override MessagePacketType Type => MessagePacketType.WorldPacket;
    }
}