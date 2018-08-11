using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.ClassSystem
{
    public class RogueDesign : ClassDesign
    {
        private static readonly uint LogoId = Graphics2D.LoadFromAssets("Assets/UI/RogueLogo.png");
        public override uint Logo { get; } = LogoId;
        public override HumanType Human => HumanType.Rogue;
        public override float BaseSpeed => 1.40f;
        public override AbilityTreeBlueprint AbilityTreeDesign => new RogueAbilityTreeBlueprint();
        public override Item StartingItem => ItemPool.Grab(CommonItems.CommonBronzeDoubleBlades);
        public override float AttackResistance => 1.5f;

        public override float MaxHealthFormula(float RandomFactor)
        {
            return 38 + ((RandomFactor - .75f) * 8 - 1f) * 5 - 2.5f;
        }

        public override float MaxManaFormula(float RandomFactor)
        {
            return 16.5f + ((RandomFactor - .75f) * 8 - 1f) * 10 - 5f;
        }
    }
}
