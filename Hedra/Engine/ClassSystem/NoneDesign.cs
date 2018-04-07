using System;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.Skills;

namespace Hedra.Engine.ClassSystem
{
    [HiddenClass]
    public class NoneDesign : ClassDesign
    {
        public override uint Logo => 0;
        public override HumanType Human
        {
            get { throw new ArgumentException("Cannot retrieve HumanType from NoneClass"); }
        }
        public override float BaseSpeed => 1.25f;
        public override AbilityTreeBlueprint AbilityTreeDesign => null;
        public override Item StartingItem => null;
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
