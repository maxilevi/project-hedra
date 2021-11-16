using Hedra.Core;
using Hedra.EntitySystem;
using Hedra.Numerics;

namespace Hedra.Components.Effects
{
    public class SpeedComponent : ApplyEffectComponent
    {
        private float _speedTime;

        public SpeedComponent(IEntity Entity, int Chance = 10, float SpeedBonus = .5f, float BaseSpeedTime = 4) : base(
            Entity, Chance, SpeedBonus, BaseSpeedTime)
        {
        }

        public override void Update()
        {
            _speedTime -= Time.IndependentDeltaTime;
        }

        protected override void DoApply(IEntity Victim, float Amount)
        {
            if (_speedTime > 0) return;
            _speedTime = Duration + Utils.Rng.NextFloat() * 2 - 1f;
            if (Parent.SearchComponent<SpeedBonusComponent>() == null)
                Parent.ComponentManager.AddComponentWhile(new SpeedBonusComponent(Parent, Damage),
                    () => _speedTime > 0);
        }
    }
}