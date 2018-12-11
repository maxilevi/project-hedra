using Hedra.Core;
using Hedra.EntitySystem;
using Hedra.Sound;
using OpenTK;

namespace Hedra.AISystem
{
    public class SheepFarmAIComponent : FarmAnimalAIComponent
    {
        public SheepFarmAIComponent(IEntity Parent, Vector3 FarmPosition, float Width) : base(Parent, FarmPosition, Width)
        {
            AlertTime = 8 + Utils.Rng.NextFloat() * 12f;
        }

        protected override float AlertTime { get; }
        protected override SoundType Sound => SoundType.Sheep;
    }
}