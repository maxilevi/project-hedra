using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;
using Hedra.Sound;
using OpenTK;

namespace Hedra.AISystem.Behaviours
{
    public class RoamBehaviour : Behaviour
    {
        protected IdleBehaviour Idle { get; }
        protected TraverseBehaviour Traverse { get; }
        private Behaviour _currentBehaviour;
        private readonly Timer _moveTicker;
        private readonly Timer _soundTicker;

        public RoamBehaviour(IEntity Parent) : base(Parent)
        {
            this.Idle = new IdleBehaviour(Parent);
            this.Traverse = new TraverseBehaviour(Parent);
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
                    var targetPosition = World.FindPlaceablePosition(Parent, SearchPoint + new Vector3(Utils.Rng.NextFloat() * Diameter - Radius, 0,
                                             Utils.Rng.NextFloat() * Diameter - Radius));
                    if (Physics.IsWaterBlock(targetPosition)) return;
                    Traverse.SetTarget(targetPosition, () => _currentBehaviour = Idle);
                    _currentBehaviour = Traverse;
                }

                if (this._soundTicker.Tick())
                {
                    SoundPlayer.PlaySoundWithVariation(Sound, Parent.Position);
                }
            }
            //if (Parent.IsStuck) Traverse.Cancel();
        }

        protected virtual Vector3 SearchPoint => Parent.Position;
        
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
