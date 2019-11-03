using System.Linq;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.EntitySystem;
using System.Numerics;

namespace Hedra.AISystem.Behaviours
{
    public class HerdBehaviour : Behaviour
    {
        public float CallRadius { get; set; } = 80f;
        public FleeBehaviour Flee { get; set; }
        public AttackBehaviour Attack { get; set; }

        public HerdBehaviour(IEntity Parent) : base(Parent)
        {
            this.Flee = new FleeBehaviour(Parent);
            this.Attack = new AttackBehaviour(Parent);
            var damageComponent = Parent.SearchComponent<DamageComponent>();
            if (damageComponent != null)
            {
                damageComponent.OnDamageEvent += E => AlertHerd(E.Victim);
            }
        }

        private void AlertHerd(IEntity Attacker)
        {
            if (Attacker == null) return;
            var nearEntities = World.InRadius<IEntity>(Parent.Position, CallRadius).Where(E => E.Type == Parent.Type).ToList();                      
            nearEntities.ForEach(delegate(IEntity E)
            {
                var baseAIComponent = E.SearchComponent<BasicAIComponent>();
                var herd = baseAIComponent.SearchBehaviour<HerdBehaviour>();
                if (herd != null)
                {
                    /*if (nearEntities.Count < 4)
                    {
                        //Check if it is an arrow
                        if ((Args.Damager.Position - Parent.Position).LengthSquared() > CallRadius * CallRadius)
                        {
                            herd.SetFlee(Parent.Position + (Args.Damager.Position - Parent.Position).NormalizedFast(), CallRadius);
                        }
                        else
                        {
                            herd.SetFlee(Args.Damager, CallRadius);
                        }
                    }
                    else
                    {*/
                    herd.SetAttack(Attacker);
                    //}
                }
            });
        }

        public override void Update()
        {
            if (Flee.Enabled)
            {
                Flee.Update();
            }
            else if(Attack.Enabled)
            {
                Attack.Update();
            }
        }

        public void SetFlee(IEntity Target, float Radius)
        {
            Flee.SetTarget(Target, Radius);
        }

        public void SetFlee(Vector3 Point, float Radius)
        {
            Flee.SetTarget(Point, Radius);
        }

        public void SetAttack(IEntity Target)
        {
            Attack.SetTarget(Target);
        }

        public bool Enabled => Attack.Enabled || Flee.Enabled;

        public override void Dispose()
        {
            Flee.Dispose();
            Attack.Dispose();
        }
    }
}
