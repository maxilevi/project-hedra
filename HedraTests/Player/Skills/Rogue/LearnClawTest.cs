using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.SkillSystem.Rogue;
using Hedra.Items;

namespace HedraTests.Player.Skills.Rogue
{
    public class LearnClawTest : LearningSkillTestBase<LearnClaw>
    {
        protected override EquipmentType LearnType => EquipmentType.Claw;
        protected override int InventoryPosition => PlayerInventory.WeaponHolder;
    }
}