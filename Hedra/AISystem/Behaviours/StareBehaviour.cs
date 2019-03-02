using System.Linq;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public class StareBehaviour : Behaviour
    {
        public StareBehaviour(IEntity Parent) : base(Parent)
        {
        }

        public override void Update()
        {
            var nearHumanoid = World.InRadius<IHumanoid>(Parent.Position, 16f).FirstOrDefault(H => !H.IsInvisible);
            if (nearHumanoid != null)
            {
                Parent.RotateTowards(nearHumanoid);
            }
        }
    }
}