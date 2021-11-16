using System.Numerics;
using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Sound;

namespace Hedra.AISystem.Mob
{
    public class SheepFarmAIComponent : FarmAnimalAIComponent
    {
        public SheepFarmAIComponent(IEntity Parent, Vector3 FarmPosition, float Width) : base(Parent, FarmPosition,
            Width)
        {
        }

        protected override float AlertTime => 8 + Utils.Rng.NextFloat() * 12f;
        protected override SoundType Sound => SoundType.Sheep;
    }
}