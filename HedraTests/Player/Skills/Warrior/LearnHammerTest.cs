using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.SkillSystem.Warrior;

namespace HedraTests.Player.Skills.Warrior
{
    public class LearnHammerTest : LearningSkillTestBase<LearnHammer>
    {
        protected override EquipmentType LearnType => EquipmentType.Hammer;
        protected override int InventoryPosition => PlayerInventory.WeaponHolder;
    }
}