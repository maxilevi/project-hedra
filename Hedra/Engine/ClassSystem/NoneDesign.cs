using System;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.AbilityTreeSystem;
using Hedra.Engine.Player.Skills;

namespace Hedra.Engine.ClassSystem
{
    [HiddenClass]
    public class NoneDesign : ClassDesign
    {
        public override string Logo => string.Empty;
        public override HumanType Human => throw new ArgumentException("Cannot retrieve HumanType from NoneClass");
        public override float BaseSpeed => 1.25f;
        public override AbilityTreeBlueprint AbilityTreeDesign => null;
        public override Item StartingItem => null;
        public override float AttackResistance => throw new ArgumentException();
        public override float MaxStamina => throw new ArgumentException();

        public override float MaxHealthFormula(float RandomFactor)
        {
            return 30 + ((RandomFactor - .75f) * 8 - 1f) * 5 - 2.5f;
        }

        public override float MaxManaFormula(float RandomFactor)
        {
            return 30 + ((RandomFactor - .75f) * 8 - 1f) * 5 - 2.5f;
        }
    }
}
