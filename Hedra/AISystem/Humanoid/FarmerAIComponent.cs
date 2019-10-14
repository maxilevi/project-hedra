using System.Timers;
using Hedra.Core;
using Hedra.Engine;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.EntitySystem;
using System.Numerics;

namespace Hedra.AISystem.Humanoid
{
    public class FarmerAIComponent : BaseVillagerAIComponent
    {
        private readonly Vector2 _farmPosition;
        private readonly Vector2 _farmSize;
        
        public FarmerAIComponent(IHumanoid Parent, Vector2 FarmPosition, Vector2 FarmSize) : base(Parent, true)
        {
            _farmPosition = FarmPosition;
            _farmSize = FarmSize - Vector2.One;
            Parent.Speed = 1f;
        }

        public override void Update()
        {
            base.Update();
            if (!IsMoving && !IsSitting && !Parent.IsAttacking)
            {
                Parent.Orientation = new Vector2(Utils.Rng.NextFloat() * 2f - 1f, Utils.Rng.NextFloat() * 2f - 1f).NormalizedFast().ToVector3();
                Parent.Model.TargetRotation = Physics.DirectionToEuler(Parent.Orientation);
                Parent.LeftWeapon.Attack1(Parent);
            }
            else
            {
                Parent.WasAttacking = false;
            }
        }

        protected override Vector3 NewPoint
        {
            get
            {
                /* Try 10 times */
                for(var i = 0; i < 10; ++i)
                {
                    var newPoint = new Vector3(
                        Utils.Rng.NextFloat() * _farmSize.X - _farmSize.X * .5f,
                        0,
                        Utils.Rng.NextFloat() * _farmSize.Y - _farmSize.Y * .5f
                    ) + _farmPosition.ToVector3();
                    if (IsSuitableSpot(newPoint))
                        return newPoint;
                }
                return Parent.Position;
            }
        }

        protected virtual bool IsSuitableSpot(Vector3 Point)
        {
            return World.GetHighestBlockAt(Point.X, Point.Z).Type == BlockType.FarmDirt;
        }
        
        protected override float WaitTime => 10.0f + Utils.Rng.NextFloat() * 8f;
    }
}