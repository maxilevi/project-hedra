using Hedra.Core;
using Hedra.EntitySystem;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.Player.BoatSystem
{
    public class BoatHumanoidInputHandler : BoatInputHandler
    {
        private const float Speed = 1.5f;
        private readonly IHumanoid _humanoid;
        private BoatStateHandler _stateHandler;
        private Vector3 _lastRotation;

        public BoatHumanoidInputHandler(IHumanoid Humanoid, BoatStateHandler StateHandler) : base(Humanoid)
        {
            _humanoid = Humanoid;
            _stateHandler = StateHandler;
            _lastRotation = Vector3.UnitY * Utils.Rng.NextFloat() * 360;
        }

        public override void Update()
        {
            const float propulsionFactor = 35.0f;
            _accumulatedDirection = _humanoid.Physics.LinearVelocity;
            if(_stateHandler.Enabled)
                _humanoid.IsSitting = true;
            if (_humanoid.IsMoving)
            {
                _lastRotation = _humanoid.Model.TargetRotation;
            }
            else
            {
                _humanoid.Model.TargetRotation = _lastRotation;   
            }
            HandleBoatRotation(propulsionFactor);
        }

        public override bool ShouldDrift { get; }

        public override Vector3 Velocity => _accumulatedDirection;
    }
}