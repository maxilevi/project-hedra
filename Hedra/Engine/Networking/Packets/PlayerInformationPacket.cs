using System.Linq;
using Hedra.API;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.Engine.Player;
using Hedra.EntitySystem;

namespace Hedra.Engine.Networking.Packets
{
    public class PlayerInformationPacket : NetworkPacket<PlayerInformationPacket>
    {
        public override MessagePacketType Type => MessagePacketType.PlayerInformationPacket;
        
        public HumanoidModelTemplate ModelTemplate { get; private set; }
        public Item[] EquipmentNames { get; private set;  }

        public static PlayerInformationPacket From(IHumanoid Humanoid)
        {
            return new PlayerInformationPacket
            {
                ModelTemplate = Humanoid.Model.Template,
                EquipmentNames = Humanoid.MainEquipment
            };
        }
        
        protected override void DoParse(PacketReader Reader, PlayerInformationPacket Empty)
        {
            Empty.ModelTemplate = (HumanoidModelTemplate) HumanoidModelTemplate.FromJson(Reader.ReadString());
            Empty.EquipmentNames = Reader.ReadArray(Reader.ReadItem);
        }

        protected override void DoSerialize(PacketWriter Writer)
        {
            Writer.Write(HumanoidModelTemplate.ToJson(ModelTemplate));
            Writer.Write(EquipmentNames, Writer.Write);
        }
    }
}