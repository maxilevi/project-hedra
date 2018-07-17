using System;
using System.Collections.Generic;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public sealed class PlacementDesigner
    {
        public PlacementDesign CreateDesign(Random Rng)
        {
            var ring = CreateBounds(Rng);
            var design = new PlacementDesign();
            design.BlacksmithCount = Rng.Next(0, 2);

            return design;
        }

        private static VillageRing CreateBounds(Random Rng)
        {
            return new VillageRing
            {
                Radius = 756,
                InnerRing = new VillageRing
                {
                    Radius = 384,
                    InnerRing = new VillageRing
                    {
                        Radius = 128
                    }
                }
            };
        }

        private static bool CollidesWithOtherPlacements(PlacementPoint Point, List<PlacementPoint> Points)
        {
            for (var i = 0; i < Points.Count; i++)
            {
                if (PlacementPoint.Collide(Point, Points[i]))
                    return false;
            }
            return true;
        }
    }

    class PlacementPoint
    {
        public Vector3 Position { get; set; }
        public float Radius { get; set; }

        public static bool Collide(PlacementPoint PointA, PlacementPoint PointB)
        {
            return (PointA.Position - PointB.Position).LengthSquared < Math.Pow(PointA.Radius + PointB.Radius, 2);
        }
    }
}