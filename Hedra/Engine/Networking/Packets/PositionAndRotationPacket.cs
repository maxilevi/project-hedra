using Hedra.EntitySystem;
using System.Numerics;

namespace Hedra.Engine.Networking.Packets
{
    public class PositionAndRotationPacket : NetworkPacket<PositionAndRotationPacket>
    {
        public override MessagePacketType Type => MessagePacketType.PositionAndRotationPacket;
        
        /* Serves as humanoid ids and mob ids */
        public ulong Id { get; set; }
        public Vector3 Position { get; private set; }
        public Vector3 Rotation { get; private set; }

        public static PositionAndRotationPacket From(IEntity Entity, ulong Id)
        {
            return new PositionAndRotationPacket
            {
                Id = Id,
                Position = Entity.Position,
                Rotation = Entity.Rotation
            };
        }
        
        protected override void DoParse(PacketReader Reader, PositionAndRotationPacket Empty)
        {
            Empty.Position = Reader.ReadVector3();
            Empty.Rotation = Reader.ReadVector3();
        }

        protected override void DoSerialize(PacketWriter Writer)
        {
            Writer.Write(Position);
            Writer.Write(Rotation);
        }
    }
}