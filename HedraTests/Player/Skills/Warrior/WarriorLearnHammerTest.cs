using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Skills;

namespace HedraTests.Player.Skills.Warrior
{
    public class WarriorLearnHammerTest : LearningSkillTestBase<WarriorLearnHammer>
    {
        protected override EquipmentType LearnType => EquipmentType.Hammer;
        protected override int InventoryPosition => PlayerInventory.WeaponHolder;
    }
}