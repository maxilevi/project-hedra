namespace Hedra.Engine.Networking.Packets
{
    public class AcceptJoinPacket : WorldPacket<AcceptJoinPacket>
    {
        public override MessagePacketType Type => MessagePacketType.AcceptJointPacket;
    }
}