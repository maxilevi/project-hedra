using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.AISystem.Behaviours
{
    public class RoamBehaviour : Behaviour
    {
        protected IdleBehaviour Idle { get; }
        protected WalkBehaviour Walk { get; }
        private Behaviour _currentBehaviour;
        private readonly Timer _ticker;

        public RoamBehaviour(Entity Parent) : base(Parent)
        {
            this.Idle = new IdleBehaviour(Parent);
            this.Walk = new WalkBehaviour(Parent);
            this._ticker = new Timer(8f);
            this._currentBehaviour = Idle;
        }

        public override void Update()
        {
            _currentBehaviour.Update();
            if (_currentBehaviour == Idle && this._ticker.Tick())
            {
                var targetPosition = Parent.Position + new Vector3(Utils.Rng.NextFloat() * Diameter - Radius, 0, Utils.Rng.NextFloat() * Diameter - Radius);
                Walk.SetTarget(targetPosition, () => _currentBehaviour = Idle);
                _currentBehaviour = Walk;
            }
        }

        private float Diameter => Radius * 2f;

        public float Radius { get; set; } = 80f;

        public float AlertTime
        {
            get => _ticker.AlertTime;
            set => _ticker.AlertTime = value;
        }
    }
}
