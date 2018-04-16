﻿using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.ClassSystem
{
    public class RogueDesign : ClassDesign
    {
        public override uint Logo { get; } = Graphics2D.LoadFromAssets("Assets/UI/RogueLogo.png");
        public override HumanType Human => HumanType.Rogue;
        public override float BaseSpeed => 1.45f;
        public override AbilityTreeBlueprint AbilityTreeDesign => new RogueAbilityTreeBlueprint();
        public override Item StartingItem => ItemPool.Grab(CommonItems.CommonBronzeDoubleBlades); 
        public override float MaxHealthFormula(float RandomFactor)
        {
            return 38 + ((RandomFactor - .75f) * 8 - 1f) * 5 - 2.5f;
        }

        public override float MaxManaFormula(float RandomFactor)
        {
            return 37.5f + ((RandomFactor - .75f) * 8 - 1f) * 10 - 5f;
        }
    }
}