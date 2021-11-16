using Hedra.Components;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public class RetaliateBehaviour : Behaviour
    {
        public RetaliateBehaviour(IEntity Parent) : base(Parent)
        {
            Attack = new AttackBehaviour(Parent);
            var damageComponent = Parent.SearchComponent<DamageComponent>();
            if (damageComponent != null)
                damageComponent.OnDamageEvent += delegate(DamageEventArgs Args)
                {
                    if (Args.Damager != null) Attack.SetTarget(Args.Damager);
                };
        }

        protected AttackBehaviour Attack { get; }

        public bool Enabled => Attack.Enabled;

        public void Draw()
        {
            Attack.Draw();
        }

        public override void Update()
        {
            Attack.Update();
        }

        public override void Dispose()
        {
            Attack.Dispose();
        }
    }
}