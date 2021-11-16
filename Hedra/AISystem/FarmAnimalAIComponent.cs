using System.Numerics;
using Hedra.AISystem.Behaviours;
using Hedra.EntitySystem;

namespace Hedra.AISystem
{
    public abstract class FarmAnimalAIComponent : FeedableCattleAIComponent
    {
        private readonly float _width;

        protected FarmAnimalAIComponent(IEntity Parent, Vector3 FarmPosition, float Width) : base(Parent)
        {
            _width = Width;
            AlterBehaviour<RoamBehaviour>(new RoamAroundBehaviour(Parent, FarmPosition));
        }

        protected override float Radius => _width * 2;
    }
}