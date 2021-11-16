using Hedra.Engine.EntitySystem;
using Hedra.Numerics;

namespace Hedra.EntitySystem
{
    public abstract class ApplyEffectComponent : EntityComponent
    {
        protected ApplyEffectComponent(IEntity Entity, int Chance, float Damage, float Duration) : base(Entity)
        {
            Parent.AfterDamaging += Apply;
        }

        public int Chance { protected get; set; }
        public float Damage { protected get; set; }
        public float Duration { protected get; set; }

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