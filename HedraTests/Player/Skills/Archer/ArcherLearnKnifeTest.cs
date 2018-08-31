using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Player.Skills.Archer;

namespace HedraTests.Player.Skills.Archer
{
    public class ArcherLearnKnifeTest : LearningSkillTestBase<LearnKnife>
    {
        protected override EquipmentType LearnType => EquipmentType.Knife;
        protected override int InventoryPosition => PlayerInventory.WeaponHolder;
    }
}