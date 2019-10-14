using System.Linq;
using Hedra.API;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Animation;
using Hedra.EntitySystem;
using Hedra.Items;
using System.Numerics;

namespace Hedra.Engine.Networking.Packets
{
    public class PlayerInformationPacket : NetworkPacket<PlayerInformationPacket>
    {
        public override MessagePacketType Type => MessagePacketType.PlayerInformationPacket;
        
        public ulong Id { get; private set; }
        public HumanoidModelTemplate ModelTemplate { get; private set; }
        public Item[] Equipment { get; private set;  }
        public Vector3 Position { get; private set; }

        public static void Apply(IHumanoid Humanoid, PlayerInformationPacket Packet)
        {
            Humanoid.SetMainEquipment(Packet.Equipment);
            var newModel = AnimationModelLoader.LoadEntity(Packet.ModelTemplate.Path);
            newModel.Scale = Vector3.One * Packet.ModelTemplate.Scale;
            var oldModel = Humanoid.Model.SwitchModel(newModel);
            oldModel.Dispose();
        }
        
        public static PlayerInformationPacket From(ulong Id, IHumanoid Humanoid)
        {
            return new PlayerInformationPacket
            {
                Id = Id,
                ModelTemplate = Humanoid.Model.Template,
                Equipment = Humanoid.GetMainEquipment(),
                Position = Humanoid.Position
            };
        }
        
        protected override void DoParse(PacketReader Reader, PlayerInformationPacket Empty)
        {
            Empty.Id = Reader.ReadUInt64();
            Empty.ModelTemplate = (HumanoidModelTemplate) HumanoidModelTemplate.FromJson(Reader.ReadString());
            Empty.Equipment = Reader.ReadArray(Reader.ReadItem);
            Empty.Position = Reader.ReadVector3();
        }

        protected override void DoSerialize(PacketWriter Writer)
        {
            Writer.Write(Id);
            Writer.Write(HumanoidModelTemplate.ToJson(ModelTemplate));
            Writer.Write(Equipment, Writer.Write);
            Writer.Write(Position);
        }
    }
}