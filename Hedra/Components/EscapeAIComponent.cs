using Hedra.Engine;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Game;
using Hedra.Engine.PhysicsSystem;
using OpenTK;

namespace Hedra.Components
{
    /// <inheritdoc />
    public class EscapeAIComponent : EntityComponent
    {
        private readonly Entity _target;
        private readonly float _speed;
        private Vector3 _targetDirection;

        public EscapeAIComponent(Entity Parent, Entity Target) : base(Parent)
        {
            _target = Target;
            _speed = _target.Speed;
        }

        public override void Update()
        {
            _targetDirection = (_target.Position - Parent.Position).Xz.ToVector3().NormalizedFast();
            Parent.Orientation = -_targetDirection;
            if ((_target.Position - Parent.Position).Xz.ToVector3().LengthFast < GameSettings.UpdateDistance * .75f)
            {
                Parent.Physics.Move();
            }
            Parent.Model.TargetRotation = Physics.DirectionToEuler( Parent.Orientation );
        }
    }
}