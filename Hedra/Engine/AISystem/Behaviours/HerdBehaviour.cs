using System.Linq;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;

namespace Hedra.Engine.AISystem.Behaviours
{
    public class HerdBehaviour : Behaviour
    {
        public float CallRadius { get; set; } = 64f;
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
                        var baseAIComponent = E.SearchComponent<BaseAIComponent>();
                        var herd = baseAIComponent.SearchBehaviour<HerdBehaviour>();
                        if (herd != null)
                        {
                            if (nearEntities.Count < 4)
                            {
                                herd.SetFlee(Args.Damager, CallRadius);
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

        public void Update()
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

        public void SetFlee(Entity Target, float Radius)
        {
            Flee.SetTarget(Target, Radius);
        }

        public void SetAttack(Entity Target)
        {
            Attack.SetTarget(Target);
        }

        public bool Enabled => Attack.Enabled || Flee.Enabled;
    }
}
