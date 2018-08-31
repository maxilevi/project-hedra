using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Skills;

namespace HedraTests.Player.Skills.Warrior
{
    public class WarriorLearnAxeTest : LearningSkillTestBase<WarriorLearnAxe>
    {
        protected override EquipmentType LearnType => EquipmentType.Axe;
        protected override int InventoryPosition => PlayerInventory.WeaponHolder;
    }
}