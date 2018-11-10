using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.ClassSystem
{
    [HiddenClass]
    public class MageDesign : ClassDesign
    {
        public override string Logo => "Assets/UI/MageLogo.png";
        public override HumanType Human => HumanType.Mage;
        public override float BaseSpeed => 1.35f;
        public override AbilityTreeBlueprint AbilityTreeDesign => new MageAbilityTreeBlueprint();
        public override Item StartingItem => ItemPool.Grab(CommonItems.CommonWoodenStaff);
        public override float AttackResistance => 0.95f;
        public override float MaxStamina => 100;
        public override float BaseDamage => 2.75f;

        public override float MaxHealthFormula(float RandomFactor)
        {
            return 7.5f + RandomFactor * 1f;
        }

        public override float MaxManaFormula(float RandomFactor)
        {
            return 11f + RandomFactor;
        }
    }
}
