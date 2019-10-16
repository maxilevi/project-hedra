using System;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.PhysicsSystem;
using Hedra.EntitySystem;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.AISystem.Humanoid
{
    public abstract class RawMovementHumanoidAIComponent : BaseHumanoidAIComponent
    {
        protected RawMovementHumanoidAIComponent(IHumanoid Entity) : base(Entity)
        {
        }
        
        /// <summary>
        /// Move to target position. Needs to be called every frame.
        /// </summary>
        /// <param name="TargetPoint">Target point to move</param>
        protected virtual void Move(Vector3 TargetPoint, float ErrorMargin = DefaultErrorMargin)
        {
            if ((TargetPoint.Xz() - Parent.Position.Xz()).LengthSquared() > ErrorMargin * ErrorMargin)
            {
                Parent.Orientation = (TargetPoint.Xz() - Parent.Position.Xz()).ToVector3().NormalizedFast();
                Parent.Model.TargetRotation = Physics.DirectionToEuler(Parent.Orientation);
                Parent.Physics.Move();
                Parent.IsSitting = false;
                IsMoving = true;
            }
            else
            {
                if(IsMoving)
                    OnTargetPointReached();
                IsMoving = false;
            }
            if (Parent.IsUnderwater)
            {
                if (Math.Abs(TargetPoint.Y - Parent.Position.Y) > 1)
                    Parent.Movement.MoveInWater(TargetPoint.Y > Parent.Position.Y);
            }
            if(!Parent.IsMoving && IsMoving)
            {
                OnMovementStuck();
            }
        }
    }
}