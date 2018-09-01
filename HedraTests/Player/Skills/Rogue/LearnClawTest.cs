using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Skills.Rogue;

namespace HedraTests.Player.Skills.Rogue
{
    public class LearnClawTest : LearningSkillTestBase<LearnClaw>
    {
        protected override EquipmentType LearnType => EquipmentType.Claw;
        protected override int InventoryPosition => PlayerInventory.WeaponHolder;
    }
}