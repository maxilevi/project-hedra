using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Player.Skills.Warrior;

namespace HedraTests.Player.Skills.Warrior
{
    public class LearnHammerTest : LearningSkillTestBase<LearnHammer>
    {
        protected override EquipmentType LearnType => EquipmentType.Hammer;
        protected override int InventoryPosition => PlayerInventory.WeaponHolder;
    }
}