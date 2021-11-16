using System.Numerics;
using Hedra.Engine;
using Hedra.Engine.PhysicsSystem;
using Hedra.EntitySystem;
using Hedra.Numerics;

namespace Hedra.AISystem.Humanoid
{
    public class NPCAIComponent : BaseVillagerAIComponent
    {
        private readonly Vector2 _center;
        private readonly float _moveRadius;

        public NPCAIComponent(IHumanoid Parent, Vector2 Center, float MoveRadius) : base(Parent, true)
        {
            _moveRadius = MoveRadius;
            _center = Center;
        }

        protected override Vector3 NewPoint
        {
            get
            {
                /* Try 10 times */
                for (var i = 0; i < 10; ++i)
                {
                    var newPoint =
                        new Vector3(Utils.Rng.NextFloat() * _moveRadius - _moveRadius * .5f, 0,
                            Utils.Rng.NextFloat() * _moveRadius - _moveRadius * .5f) + _center.ToVector3();
                    if (!Parent.Physics.CollidesWithOffset(-Parent.Position +
                                                           new Vector3(newPoint.X, Physics.HeightAtPosition(newPoint),
                                                               newPoint.Z)))
                        return newPoint;
                }

                return Parent.Position;
            }
        }
    }
}