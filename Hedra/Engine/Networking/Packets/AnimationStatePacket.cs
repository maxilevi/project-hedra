using Hedra.EntitySystem;

namespace Hedra.Engine.Networking.Packets
{
    public class AnimationStatePacket : NetworkPacket<AnimationStatePacket>
    {
        public override MessagePacketType Type => MessagePacketType.AnimationStatePacket;
        
        public bool[] AnimationStates { get; private set; }

        public static AnimationStatePacket From(IHumanoid Humanoid)
        {
            return new AnimationStatePacket
            {
                AnimationStates = Humanoid.Model.StateHandler.ModifiableStates
            };
        }

        public static void Apply(AnimationStatePacket Packet, IHumanoid Humanoid)
        {
            Humanoid.Model.StateHandler.ModifiableStates = Packet.AnimationStates;
        }
        
        protected override void DoParse(PacketReader Reader, AnimationStatePacket Empty)
        {
            Empty.AnimationStates = Reader.ReadArray(Reader.ReadBoolean);
        }

        protected override void DoSerialize(PacketWriter Writer)
        {
            Writer.Write(AnimationStates, Writer.Write);
        }
    }
}