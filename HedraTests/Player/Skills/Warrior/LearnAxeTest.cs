using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Player.Skills.Warrior;

namespace HedraTests.Player.Skills.Warrior
{
    public class LearnAxeTest : LearningSkillTestBase<LearnAxe>
    {
        protected override EquipmentType LearnType => EquipmentType.Axe;
        protected override int InventoryPosition => PlayerInventory.WeaponHolder;
    }
}