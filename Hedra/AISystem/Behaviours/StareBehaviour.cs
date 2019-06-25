using System.Linq;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public class StareBehaviour : Behaviour
    {
        private readonly Timer _timer;
        private IHumanoid _lookingAt;
        
        public StareBehaviour(IEntity Parent) : base(Parent)
        {
            _timer = new Timer(.5f);
        }

        public override void Update()
        {
            if(_timer.Tick()) _lookingAt = World.InRadius<IHumanoid>(Parent.Position, 16f).FirstOrDefault(H => !H.IsInvisible);
            if (_lookingAt != null)
            {
                Parent.RotateTowards(_lookingAt);
            }
        }
    }
}