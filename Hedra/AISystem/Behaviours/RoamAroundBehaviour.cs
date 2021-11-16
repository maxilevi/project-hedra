using System.Numerics;
using Hedra.EntitySystem;

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