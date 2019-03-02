using System;
using Hedra.AISystem.Behaviours;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.AISystem.Humanoid
{
    public abstract class TraverseHumanoidAIComponent : BaseHumanoidAIComponent
    {
        private readonly TraverseBehaviour _traverse;
        
        protected TraverseHumanoidAIComponent(IHumanoid Entity) : base(Entity)
        {
            _traverse = new TraverseBehaviour(Entity, UseCollision);
        }

        public override void Update()
        {
            base.Update();
            UpdateMovement();
            _traverse.Update();
        }

        /// <summary>
        /// Move to target position. Needs to be called every frame.
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

        protected void CancelMovement()
        {
            _traverse.Cancel();
        }

        private void UpdateMovement()
        {
            if(!_traverse.HasTarget) return;
            Parent.IsSitting = false;
            IsMoving = true;         
            if (Parent.IsUnderwater)
            {
                if (Math.Abs(_traverse.Target.Y - Parent.Position.Y) > 1)
                    Parent.Movement.MoveInWater(_traverse.Target.Y > Parent.Position.Y);
            }
            if(Parent.IsStuck)
            {
                OnMovementStuck();
            }
        }
        
        public float ErrorMargin
        {
            get => _traverse.ErrorMargin;
            set => _traverse.ErrorMargin = value;
        }
        
        protected virtual bool UseCollision => false;
    }
}