using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Sound;

namespace Hedra.AISystem.Behaviours
{
    public class RoamBehaviour : Behaviour
    {
        private readonly Timer _moveTicker;
        private readonly Timer _soundTicker;
        private Behaviour _currentBehaviour;

        public RoamBehaviour(IEntity Parent) : base(Parent)
        {
            Idle = new IdleBehaviour(Parent);
            Traverse = new TraverseBehaviour(Parent);
            _moveTicker = new Timer(8f + Utils.Rng.NextFloat() * 4);
            _soundTicker = new Timer(5f + Utils.Rng.NextFloat() * 5);
            _currentBehaviour = Idle;
        }

        protected IdleBehaviour Idle { get; }
        protected TraverseBehaviour Traverse { get; }

        protected virtual Vector3 SearchPoint => Parent.Position;

        private float Diameter => Radius * 2f;

        public float Radius { get; set; } = 80f;

        public SoundType Sound { get; set; }

        public float AlertTime
        {
            get => _moveTicker.AlertTime;
            set => _moveTicker.AlertTime = value;
        }

        public override void Update()
        {
            _currentBehaviour.Update();
            if (_currentBehaviour == Idle)
            {
                if (_moveTicker.Tick())
                {
                    var randomPosition = SearchPoint + new Vector3(Utils.Rng.NextFloat() * Diameter - Radius, 0,
                        Utils.Rng.NextFloat() * Diameter - Radius);
                    var targetPosition = World.FindPlaceablePosition(Parent,
                        new Vector3(randomPosition.X,
                            Physics.HeightAtPosition(randomPosition.X, randomPosition.Z) + Chunk.BlockSize * 2,
                            randomPosition.Z));
                    if (Physics.IsWaterBlock(targetPosition)) return;
                    Traverse.SetTarget(targetPosition, () => _currentBehaviour = Idle);
                    _currentBehaviour = Traverse;
                }

                if (_soundTicker.Tick()) SoundPlayer.PlaySoundWithVariation(Sound, Parent.Position);
            }
            //if (Parent.IsStuck) Traverse.Cancel();
        }

        public void Draw()
        {
            if (!Traverse.HasTarget) return;
            BasicGeometry.DrawLine(Parent.Position, Traverse.Target, Vector4.One, 2);
            BasicGeometry.DrawPoint(Traverse.Target, Vector4.One);
        }

        public override void Dispose()
        {
            Idle.Dispose();
            Traverse.Dispose();
        }
    }
}