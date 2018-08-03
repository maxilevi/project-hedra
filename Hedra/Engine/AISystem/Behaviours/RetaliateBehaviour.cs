using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.AISystem.Behaviours
{
    public class RetaliateBehaviour : Behaviour
    {
        protected AttackBehaviour Attack { get; }

        public RetaliateBehaviour(IEntity Parent) : base(Parent)
        {
            Attack = new AttackBehaviour(Parent);
            var damageComponent = Parent.SearchComponent<DamageComponent>();
            if (damageComponent != null)
            {
                damageComponent.OnDamageEvent += delegate(DamageEventArgs Args)
                {
                    if (Args.Damager != null)
                    {
                        Attack.SetTarget(Args.Damager);
                    }
                };
            }
        }

        public override void Update()
        {
            Attack.Update();
        }

        public bool Enabled => Attack.Enabled;
    }
}
