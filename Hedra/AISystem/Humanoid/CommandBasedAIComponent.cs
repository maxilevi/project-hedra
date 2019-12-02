using System.Numerics;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Humanoid
{
    public class CommandBasedAIComponent : TraverseHumanoidAIComponent
    {
        public CommandBasedAIComponent(IHumanoid Entity) : base(Entity)
        {

        }

        public void WalkTo(Vector3 TargetPoint, float ErrorMargin = DefaultErrorMargin)
        {
            base.MoveTo(TargetPoint, ErrorMargin);
        }
        
        protected override bool ShouldSleep => false;
    }
}