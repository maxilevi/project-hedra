using System;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    internal class PlacementPoint
    {
        public Vector3 Position { get; set; }
        public float Radius { get; set; }

        public static bool Collide(PlacementPoint PointA, PlacementPoint PointB)
        {
            return (PointA.Position - PointB.Position).LengthSquared < Math.Pow(PointA.Radius + PointB.Radius, 2);
        }
    }
}