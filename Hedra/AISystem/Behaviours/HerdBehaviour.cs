using System.Linq;
using System.Numerics;
using Hedra.Components;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public class HerdBehaviour : Behaviour
    {
        public HerdBehaviour(IEntity Parent) : base(Parent)
        {
            Flee = new FleeBehaviour(Parent);
            Attack = new AttackBehaviour(Parent);
            var damageComponent = Parent.SearchComponent<DamageComponent>();
            if (damageComponent != null) damageComponent.OnDamageEvent += E => AlertHerd(E.Damager);
        }

        public float CallRadius { get; set; } = 80f;
        public FleeBehaviour Flee { get; set; }
        public AttackBehaviour Attack { get; set; }

        public bool Enabled => Attack.Enabled || Flee.Enabled;

        private void AlertHerd(IEntity Attacker)
        {
            if (Attacker == null) return;
            var nearEntities = World.InRadius<IEntity>(Parent.Position, CallRadius).Where(E => E.Type == Parent.Type)
                .ToList();
            nearEntities.ForEach(delegate(IEntity E)
            {
                var baseAIComponent = E.SearchComponent<BasicAIComponent>();
                var herd = baseAIComponent.SearchBehaviour<HerdBehaviour>();
                if (herd != null)
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
            });
        }

        public void Draw()
        {
            if (Flee.Enabled)
                Flee.Draw();
            else if (Attack.Enabled) Attack.Draw();
        }

        public override void Update()
        {
            if (Flee.Enabled)
                Flee.Update();
            else if (Attack.Enabled) Attack.Update();
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

        public override void Dispose()
        {
            Flee.Dispose();
            Attack.Dispose();
        }
    }
}