using Hedra.Engine.EntitySystem;
using Hedra.Engine.PhysicsSystem;
using OpenTK;

namespace Hedra.Engine.WorldBuilding
{
    /// <inheritdoc />
    internal class EscapeAIComponent : EntityComponent
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
            if ((_target.Position - Parent.Position).Xz.ToVector3().LengthFast < GameSettings.UpdateDistance * .75f)
            {
                Parent.Model.Run();
                Parent.Physics.DeltaTranslate(-_targetDirection * _speed * 16f);
            }
            else
            {
                Parent.Model.Idle();
            }

            Parent.Orientation = -_targetDirection;
			Parent.Model.TargetRotation = Physics.DirectionToEuler( Parent.Orientation );
        }
    }
}