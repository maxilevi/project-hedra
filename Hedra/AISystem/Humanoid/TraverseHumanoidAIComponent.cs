using System;
using System.Numerics;
using Hedra.AISystem.Behaviours;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Game;

namespace Hedra.AISystem.Humanoid
{
    public abstract class TraverseHumanoidAIComponent : BaseHumanoidAIComponent
    {
        private readonly TraverseBehaviour _traverse;

        protected TraverseHumanoidAIComponent(IHumanoid Entity) : base(Entity)
        {
            _traverse = new TraverseBehaviour(Entity, UseCollision);
        }

        public float ErrorMargin
        {
            get => _traverse.ErrorMargin;
            set => _traverse.ErrorMargin = value;
        }

        protected virtual bool UseCollision => false;

        public override void Update()
        {
            base.Update();
            UpdateMovement();
            _traverse.Update();
        }

        /// <summary>
        ///     Move to target position. Needs to be called every frame.
        /// </summary>
        /// <param name="TargetPoint">Target point to move</param>
        protected void MoveTo(Vector3 TargetPoint, float ErrorMargin = DefaultErrorMargin)
        {
            _traverse.SetTarget(TargetPoint, () =>
            {
                OnTargetPointReached();
                IsMoving = false;
            });
        }

        public override void Draw()
        {
            base.Draw();
            if (!GameSettings.DebugAI) return;
            BasicAIComponent.DrawDebugCollision(Parent);
            if (!_traverse.HasTarget) return;
            BasicGeometry.DrawLine(Parent.Position + Vector3.UnitY, _traverse.Target + Vector3.UnitY, Vector4.One, 2);
            BasicGeometry.DrawPoint(_traverse.Target, Vector4.One);
        }

        protected void CancelMovement()
        {
            _traverse.Cancel();
        }

        private void UpdateMovement()
        {
            if (!_traverse.HasTarget) return;
            Parent.IsSitting = false;
            IsMoving = true;
            if (Parent.IsUnderwater)
                if (Math.Abs(_traverse.Target.Y - Parent.Position.Y) > 1)
                    Parent.Movement.MoveInWater(_traverse.Target.Y > Parent.Position.Y);
            if (Parent.IsStuck) OnMovementStuck();
        }

        public override void Dispose()
        {
            base.Dispose();
            _traverse.Dispose();
        }
    }
}