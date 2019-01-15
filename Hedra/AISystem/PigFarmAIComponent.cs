using Hedra.Core;
using Hedra.EntitySystem;
using Hedra.Sound;
using OpenTK;

namespace Hedra.AISystem
{
    public class PigFarmAIComponent : FarmAnimalAIComponent
    {
        public PigFarmAIComponent(IEntity Parent, Vector3 FarmPosition, float Width) : base(Parent, FarmPosition, Width)
        {
            AlertTime = 10 + Utils.Rng.NextFloat() * 14f;
        }

        protected override float AlertTime { get; }
        protected override SoundType Sound => SoundType.None;
    }
}