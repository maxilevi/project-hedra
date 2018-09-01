using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Skills.Rogue;

namespace HedraTests.Player.Skills.Rogue
{
    public class LearnKatarTest : LearningSkillTestBase<LearnKatar>
    {
        protected override EquipmentType LearnType => EquipmentType.Katar;
        protected override int InventoryPosition => PlayerInventory.WeaponHolder;
    }
}