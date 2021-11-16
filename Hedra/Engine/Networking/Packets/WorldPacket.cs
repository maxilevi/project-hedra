namespace Hedra.Engine.Networking.Packets
{
    public class WorldPacket : WorldPacket<WorldPacket>
    {
    }

    public interface IWorldPacket
    {
        int Seed { get; set; }
    }

    public abstract class WorldPacket<T> : NetworkPacket<T>, IWorldPacket where T : IWorldPacket, new()
    {
        public override MessagePacketType Type => MessagePacketType.WorldPacket;
        public int Seed { get; set; }

        protected override void DoParse(PacketReader Reader, T Empty)
        {
            Empty.Seed = Reader.ReadInt32();
        }

        protected override void DoSerialize(PacketWriter Writer)
        {
            Writer.Write(Seed);
        }
    }
}