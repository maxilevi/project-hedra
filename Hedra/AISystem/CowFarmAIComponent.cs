using Hedra.Core;
using Hedra.EntitySystem;
using Hedra.Sound;
using OpenTK;

namespace Hedra.AISystem
{
    public class CowFarmAIComponent : FarmAnimalAIComponent
    {
        public CowFarmAIComponent(IEntity Parent, Vector3 FarmPosition, float Width) : base(Parent, FarmPosition, Width)
        {
            AlertTime = 9 + Utils.Rng.NextFloat() * 14f;
        }

        protected override float AlertTime { get; }
        protected override SoundType Sound => SoundType.None;
    }
}