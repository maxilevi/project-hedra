using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.SkillSystem.Warrior;
using Hedra.Engine.SkillSystem.Warrior.Berserker;

namespace HedraTests.Player.Skills.Warrior
{
    public class LearnAxeTest : LearningSkillTestBase<LearnAxe>
    {
        protected override EquipmentType LearnType => EquipmentType.Axe;
        protected override int InventoryPosition => PlayerInventory.WeaponHolder;
    }
}