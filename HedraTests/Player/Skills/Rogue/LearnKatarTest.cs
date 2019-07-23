using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.SkillSystem.Rogue;
using Hedra.Items;

namespace HedraTests.Player.Skills.Rogue
{
    public class LearnKatarTest : LearningSkillTestBase<LearnKatar>
    {
        protected override EquipmentType LearnType => EquipmentType.Katar;
        protected override int InventoryPosition => PlayerInventory.WeaponHolder;
    }
}