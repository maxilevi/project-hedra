using Hedra.EntitySystem;

namespace Hedra.Components.Effects
{
    public class ToxicComponent : ApplyEffectComponent
    {
        public ToxicComponent(IEntity Entity, int Chance, float Damage, float Duration) : base(Entity, Chance, Damage,
            Duration)
        {
            Parent.AddComponent(new FoodPoisonComponent(Parent, null, Duration, Damage));
        }

        protected override void DoApply(IEntity Victim, float Amount)
        {
        }
    }
}