using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.ClassSystem
{
    internal class WarriorDesign : ClassDesign
    {
        public override bool CanAccumulateHits { get; } = false;
        public override uint Logo { get; } = Graphics2D.LoadFromAssets("Assets/UI/WarriorLogo.png");
        public override HumanType Human => HumanType.Warrior;
        public override float BaseSpeed => 1.25f;
        public override AbilityTreeBlueprint AbilityTreeDesign => new WarriorAbilityTreeBlueprint();
        public override Item StartingItem => ItemPool.Grab(CommonItems.CommonBronzeSword);
        public override float MaxHealthFormula(float RandomFactor)
        {
            return 46 + ((RandomFactor - .75f) * 8 - 1f) * 5 - 2.5f;
        }

        public override float MaxManaFormula(float RandomFactor)
        {
            return 8.0f + ((RandomFactor - .75f) * 8 - 1f) * 10 - 5f;
        }
    }
}
