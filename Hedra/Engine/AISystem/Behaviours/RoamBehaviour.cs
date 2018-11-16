using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Sound;
using OpenTK;

namespace Hedra.Engine.AISystem.Behaviours
{
    public class RoamBehaviour : Behaviour
    {
        protected IdleBehaviour Idle { get; }
        protected WalkBehaviour Walk { get; }
        private Behaviour _currentBehaviour;
        private readonly Timer _moveTicker;
        private readonly Timer _soundTicker;

        public RoamBehaviour(Entity Parent) : base(Parent)
        {
            this.Idle = new IdleBehaviour(Parent);
            this.Walk = new WalkBehaviour(Parent);
            this._moveTicker = new Timer(8f + Utils.Rng.NextFloat() * 4);
            this._soundTicker = new Timer(5f + Utils.Rng.NextFloat() * 5);
            this._currentBehaviour = Idle;
        }

        public override void Update()
        {
            _currentBehaviour.Update();
            if (_currentBehaviour == Idle)
            {
                if (this._moveTicker.Tick())
                {
                    var targetPosition = Parent.Position + new Vector3(Utils.Rng.NextFloat() * Diameter - Radius, 0,
                                             Utils.Rng.NextFloat() * Diameter - Radius);
                    if (Physics.IsWaterBlock(targetPosition)) return;
                    Walk.SetTarget(targetPosition, () => _currentBehaviour = Idle);
                    _currentBehaviour = Walk;
                }

                if (this._soundTicker.Tick())
                {
                    SoundManager.PlaySoundWithVariation(Sound, Parent.Position);
                }
            }
        }

        private float Diameter => Radius * 2f;

        public float Radius { get; set; } = 80f;
        
        public SoundType Sound { get; set; }

        public float AlertTime
        {
            get => _moveTicker.AlertTime;
            set => _moveTicker.AlertTime = value;
        }
    }
}
