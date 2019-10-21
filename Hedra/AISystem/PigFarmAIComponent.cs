using Hedra.Core;
using Hedra.EntitySystem;
using Hedra.Sound;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.AISystem
{
    public class PigFarmAIComponent : FarmAnimalAIComponent
    {
        public PigFarmAIComponent(IEntity Parent, Vector3 FarmPosition, float Width) : base(Parent, FarmPosition, Width)
        {
        }

        protected override float AlertTime => 10 + Utils.Rng.NextFloat() * 14f;
        protected override SoundType Sound => SoundType.None;
    }
}