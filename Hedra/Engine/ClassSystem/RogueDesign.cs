using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.ClassSystem
{
    public class RogueDesign : ClassDesign
    {
        public override string Logo => "Assets/UI/RogueLogo.png";
        public override HumanType Human => HumanType.Rogue;
        public override float BaseSpeed => 1.45f;
        public override AbilityTreeBlueprint AbilityTreeDesign => new RogueAbilityTreeBlueprint();
        public override Item StartingItem => ItemPool.Grab(CommonItems.CommonBronzeDoubleBlades);
        public override float AttackResistance => 1.025f;
        public override float MaxStamina => 125f;
        public override float BaseDamage => 3.5f;

        public override float MaxHealthFormula(float RandomFactor)
        {
            return 8.5f + RandomFactor * 1;
        }

        public override float MaxManaFormula(float RandomFactor)
        {
            return 8.5f + RandomFactor;
        }
    }
}
