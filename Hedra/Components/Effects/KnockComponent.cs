using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;

namespace Hedra.Components.Effects
{
    /// <inheritdoc cref="ApplyEffectComponent" />
    /// <summary>
    /// Description of FireComponent.
    /// </summary>
    public class KnockComponent : ApplyEffectComponent
    {
        private float _cooldown;

        public override void Update()
        {
            _cooldown -= Time.IndependentDeltaTime;
        }

        protected override void DoApply(IEntity Victim, float Amount)
        {
            if (_cooldown > 0) return;

            bool shouldKnock = Utils.Rng.NextFloat() <= Chance * 0.01f;

            if (!shouldKnock) return;
            if (!Victim.IsKnocked) Victim.KnockForSeconds(Duration);
            _cooldown = Duration + 2;
        }

        public KnockComponent(IEntity Entity, int Chance, float Damage, float Duration) : base(Entity, Chance, Damage, Duration)
        {
        }
    }
}
