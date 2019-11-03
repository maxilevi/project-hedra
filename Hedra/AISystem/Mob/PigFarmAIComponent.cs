using System.Numerics;
using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Sound;

namespace Hedra.AISystem.Mob
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