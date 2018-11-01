using System;
using Hedra.Engine.ItemSystem;
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
        public override float AttackResistance => 0.975f;
        public override float MaxStamina => 115;

        public override float MaxHealthFormula(float RandomFactor)
        {
            return 8f + RandomFactor * 1f;
        }

        public override float MaxManaFormula(float RandomFactor)
        {
            return 9f + RandomFactor;
        }
    }
}
