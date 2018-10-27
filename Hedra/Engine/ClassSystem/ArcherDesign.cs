﻿using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.ClassSystem
{
    public class ArcherDesign : ClassDesign
    {
        public override string Logo => "Assets/UI/ArcherLogo.png";
        public override HumanType Human => HumanType.Archer;
        public override float BaseSpeed => 1.35f;
        public override AbilityTreeBlueprint AbilityTreeDesign => new ArcherAbilityTreeBlueprint();
        public override Item StartingItem => ItemPool.Grab(CommonItems.CommonWoodenBow);
        public override float AttackResistance => 1.0f;

        public override float MaxHealthFormula(float RandomFactor)
        {
            return 22f + ((RandomFactor - .75f) * 8 - 1f) * 5 - 2.5f;
        }

        public override float MaxManaFormula(float RandomFactor)
        {
            return 12.5f + ((RandomFactor - .75f) * 8 - 1f) * 10 - 5f;
        }
    }
}
