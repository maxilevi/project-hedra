using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.AISystem.Behaviours
{
    public class RoamAroundBehaviour : RoamBehaviour
    {
        public RoamAroundBehaviour(IEntity Parent, Vector3 SearchPoint) : base(Parent)
        {
            this.SearchPoint = SearchPoint;
        }

        protected override Vector3 SearchPoint { get; }
    }
}