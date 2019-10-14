using Hedra.Core;
using Hedra.EntitySystem;
using Hedra.Sound;
using System.Numerics;

namespace Hedra.AISystem
{
    public class CowFarmAIComponent : FarmAnimalAIComponent
    {
        public CowFarmAIComponent(IEntity Parent, Vector3 FarmPosition, float Width) : base(Parent, FarmPosition, Width)
        {
        }

        protected override float AlertTime => 9 + Utils.Rng.NextFloat() * 14f;
        protected override SoundType Sound => SoundType.None;
    }
}