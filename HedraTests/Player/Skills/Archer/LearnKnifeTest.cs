using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.SkillSystem.Archer;
using Hedra.Items;

namespace HedraTests.Player.Skills.Archer
{
    public class LearnKnifeTest : LearningSkillTestBase<LearnKnife>
    {
        protected override EquipmentType LearnType => EquipmentType.Knife;
        protected override int InventoryPosition => PlayerInventory.WeaponHolder;
    }
}