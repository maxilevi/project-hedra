using System.Linq;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.AISystem.Behaviours
{
    public class HerdBehaviour : Behaviour
    {
        public float CallRadius { get; set; } = 80f;
        public FleeBehaviour Flee { get; set; }
        public AttackBehaviour Attack { get; set; }

        public HerdBehaviour(Entity Parent) : base(Parent)
        {
            this.Flee = new FleeBehaviour(Parent);
            this.Attack = new AttackBehaviour(Parent);
            var damageComponent = Parent.SearchComponent<DamageComponent>();
            if (damageComponent != null)
            {
                damageComponent.OnDamageEvent += delegate (DamageEventArgs Args)
                {
                    if (Args.Damager == null) return;
                    var nearEntities = World.InRadius<Entity>(Parent.Position, CallRadius).Where(E => E.Type == Parent.Type).ToList();                      
                    nearEntities.ForEach(delegate(Entity E)
                    {
                        var baseAIComponent = E.SearchComponent<BasicAIComponent>();
                        var herd = baseAIComponent.SearchBehaviour<HerdBehaviour>();
                        if (herd != null)
                        {
                            if (nearEntities.Count < 4)
                            {
                                //Check if it is an arrow
                                if ((Args.Damager.Position - Parent.Position).LengthSquared > CallRadius * CallRadius)
                                {
                                    herd.SetFlee(Parent.Position + (Args.Damager.Position - Parent.Position).NormalizedFast(), CallRadius);
                                }
                                else
                                {
                                    herd.SetFlee(Args.Damager, CallRadius);
                                }
                            }
                            else
                            {
                                herd.SetAttack(Args.Damager);
                            }
                        }
                    });
                };
            }
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
    }
}
