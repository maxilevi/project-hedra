using System;
using System.Linq;

using Hedra.AISystem.Behaviours;
using Hedra.EntitySystem;
using System.Numerics;
using Hedra.Engine.ItemSystem;
using Timer = Hedra.Core.Timer;

namespace Hedra.AISystem
{
    public abstract class FarmAnimalAIComponent : FeedableCattleAIComponent
    {
        private readonly float _width;

        protected FarmAnimalAIComponent(IEntity Parent, Vector3 FarmPosition, float Width) : base(Parent)
        {
            _width = Width;
            this.AlterBehaviour<RoamBehaviour>(new RoamAroundBehaviour(Parent, FarmPosition));
        }

        protected override float Radius => _width * 2;
    }
}