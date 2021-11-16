using System.Linq;
using System.Numerics;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem
{
    public static class StructureContentHelper
    {
        public static Chest AddRewardChest(Vector3 Position, VertexData Model, Item Item)
        {
            var chest = World.SpawnChest(Position, Item);
            chest.Condition = () => IsNotNearEnemies(chest.Position);
            var triangle = Model.Vertices;
            var direction = Vector3.Zero;
            for (var h = 0; h < 3; ++h)
            {
                var i = h;
                var j = (h + 1) % 3;
                var k = (h + 2) % 3;
                var ij = (triangle[i] - triangle[j]).LengthFast();
                var ik = (triangle[i] - triangle[k]).LengthFast();
                var jk = (triangle[k] - triangle[j]).LengthFast();
                if (ij < ik && ij < jk)
                {
                    var avg = (triangle[i] + triangle[j]) / 2;
                    direction = (avg - triangle[k]).NormalizedFast();
                    break;
                }
            }

            chest.Rotation = Physics.DirectionToEuler(direction) + 90 * Vector3.UnitY;
            return chest;
        }

        public static bool IsNotNearEnemies(Vector3 Position)
        {
            var mobs = World.Entities;
            return mobs.Count(M =>
                M.Distance(Position) < 32 && M.SearchComponent<IsStructureMemberComponent>() != null) == 0;
        }

        public static bool IsNotNearLookingEnemies(Vector3 Position)
        {
            var mobs = World.Entities;
            return mobs.Count(M =>
                M.Distance(Position) < 32 && !M.Physics.StaticRaycast(Position) &&
                M.SearchComponent<IsStructureMemberComponent>() != null) == 0;
        }
    }
}