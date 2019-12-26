using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;
using Hedra.EntitySystem;

namespace Hedra.Components.Effects
{
    public abstract class DamagingEffectComponent : EntityComponent
    {
        protected float TotalTime { get; set; }
        protected float TotalDamage { get; set; }
        protected IEntity Damager { get; set; }

        public virtual DamageType DamageType => DamageType.Unknown;
        
        protected DamagingEffectComponent(IEntity Entity, float TotalTime, float TotalDamage, IEntity Damager) : base(Entity)
        {
            this.TotalTime = TotalTime;
            this.TotalDamage = TotalDamage;
            this.Damager = Damager;
        }

        protected void Damage()
        {
            Parent.Damage(TotalDamage / TotalTime, Damager, out var exp, out _, true, true, DamageType);
            if(Damager is Humanoid humanoid)
                humanoid.XP += exp;
        }
    }
}