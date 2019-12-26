using Hedra.EntitySystem;

namespace Hedra.Components.Effects
{
    public class FoodPoisonComponent : PoisonComponent
    {
        public FoodPoisonComponent(IEntity Parent, IEntity Damager, float TotalTime, float TotalDamage) : base(Parent, Damager, TotalTime, TotalDamage)
        {
        }

        public override DamageType DamageType => DamageType.FoodPoison;
    }
}