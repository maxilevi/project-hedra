using Hedra.Engine.EntitySystem;
using Hedra.Numerics;

namespace Hedra.EntitySystem
{
    public abstract class ApplyEffectComponent : EntityComponent
    {
        protected ApplyEffectComponent(IEntity Entity, int Chance, float Damage, float Duration) : base(Entity)
        {
            Parent.AfterDamaging += Apply;
            this.Chance = Chance;
            this.Damage = Damage;
            this.Duration = Duration;
        }

        protected int Chance { get; }
        protected float Damage { get; }
        protected float Duration { get; }

        public override void Update()
        {
        }

        private void Apply(IEntity Victim, float Amount)
        {
            if (!(Utils.Rng.NextFloat() <= Chance * 0.01)) return;
            DoApply(Victim, Amount);
        }

        protected abstract void DoApply(IEntity Victim, float Amount);

        public override void Dispose()
        {
            base.Dispose();
            Parent.AfterDamaging -= Apply;
        }
    }
}