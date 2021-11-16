using Hedra.EntitySystem;

namespace Hedra.Engine.Networking.Packets
{
    public class PlayerAttributesPacket : NetworkPacket<PlayerAttributesPacket>
    {
        public override MessagePacketType Type => MessagePacketType.PlayerAttributesPacket;

        private float BonusHealth { get; set; }
        private float Health { get; set; }
        private int Level { get; set; }
        private float Speed { get; set; }
        private string Name { get; set; }
        private bool HandLampEnabled { get; set; }

        public static void Apply(PlayerAttributesPacket Packet, IHumanoid Humanoid)
        {
            Humanoid.BonusHealth = Packet.BonusHealth;
            Humanoid.Speed = Packet.Speed;
            Humanoid.Health = Packet.Health;
            Humanoid.Name = Packet.Name;
            Humanoid.Level = Packet.Level;
            Humanoid.HandLamp.Enabled = Packet.HandLampEnabled;
        }

        public static PlayerAttributesPacket From(IHumanoid Humanoid)
        {
            return new PlayerAttributesPacket
            {
                BonusHealth = Humanoid.BonusHealth,
                Health = Humanoid.Health,
                Level = Humanoid.Level,
                Speed = Humanoid.Speed,
                Name = Humanoid.Name,
                HandLampEnabled = Humanoid.HandLamp.Enabled
            };
        }

        protected override void DoParse(PacketReader Reader, PlayerAttributesPacket Empty)
        {
            BonusHealth = Reader.ReadSingle();
            Health = Reader.ReadSingle();
            Level = Reader.ReadInt32();
            Speed = Reader.ReadSingle();
            Name = Reader.ReadString();
            HandLampEnabled = Reader.ReadBoolean();
        }

        protected override void DoSerialize(PacketWriter Writer)
        {
            Writer.Write(BonusHealth);
            Writer.Write(Health);
            Writer.Write(Level);
            Writer.Write(Speed);
            Writer.Write(Name);
            Writer.Write(HandLampEnabled);
        }
    }
}