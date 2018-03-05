using Hedra.Engine.EntitySystem;
using Hedra.Engine.PhysicsSystem;
using OpenTK;

namespace Hedra.Engine.QuestSystem
{
    public abstract class HumanoidAIComponent : EntityComponent
    {
        protected HumanoidAIComponent(Entity Entity) : base(Entity) {}

        protected void Orientate(Vector3 TargetPoint)
        {
            Parent.Orientation = (TargetPoint - Parent.Position).Xz.NormalizedFast().ToVector3();
            Parent.Model.TargetRotation = Physics.DirectionToEuler(Parent.Orientation);
        }

        /// <summary>
        /// Move to target position. Needs to be called every frame.
        /// </summary>
        /// <param name="TargetPoint">Target point to move</param>
        protected void Move(Vector3 TargetPoint)
        {

            if ((TargetPoint.Xz - Parent.Position.Xz).LengthSquared > 3 * 3)
            {
                Parent.Physics.Move(Parent.Orientation * Parent.Speed * 5 * 2);
                Parent.Model.Run();
            }
            else
            {
                Parent.Model.Idle();
            }
        }
    }
}
