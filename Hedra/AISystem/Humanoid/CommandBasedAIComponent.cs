using System.Numerics;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Humanoid
{
    public class CommandBasedAIComponent : TraverseHumanoidAIComponent
    {
        public CommandBasedAIComponent(IHumanoid Entity) : base(Entity)
        {
        }

        protected override bool ShouldSleep => false;

        public void WalkTo(Vector3 TargetPoint, float ErrorMargin = DefaultErrorMargin)
        {
            MoveTo(TargetPoint, ErrorMargin);
        }
    }
}