namespace Hedra.Engine.Networking.Packets
{
    public class AcceptJoinPacket : WorldPacket
    {
        public override MessagePacketType Type => MessagePacketType.AcceptJointPacket;
    }
}