using System.Linq;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;

namespace Hedra.Engine.AISystem.Behaviours
{
    public class HostileBehaviour : Behaviour
    {
        public float Radius { get; } = 96;
        protected AttackBehaviour Attack { get; }

        public HostileBehaviour(Entity Parent) : base(Parent)
        {
            Attack = new AttackBehaviour(Parent);
        }

        public override void Update()
        {
            if (Attack.Target == null)
            {
                var nearPlayer = World.InRadius<LocalPlayer>(Parent.Position, Radius);
                if (nearPlayer.Length > 0)
                {
                    Attack.SetTarget(nearPlayer.First());
                }
            }
            Attack.Update();   
        }

        public bool Enabled => Attack.Enabled;
    }
}
