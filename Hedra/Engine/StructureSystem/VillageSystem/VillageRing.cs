using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    internal class VillageRing
    {
        public float Radius { get; set; }
        public VillageRing InnerRing { get; set; }

        public bool Collides(PlacementPoint Point)
        {
            return VillageRing.Collide(this, Point);
        }
        
        public static bool Collide(VillageRing Ring, PlacementPoint Point)
        {
            return Point.Position.LengthSquared < Ring.Radius * Ring.Radius &&
                   (Ring.InnerRing == null || !Ring.InnerRing.Collides(Point));
        }

        public static VillageRing Default => new VillageRing
        {
            Radius = 800,
            InnerRing = new VillageRing
            {
                Radius = 512,
                InnerRing = new VillageRing
                {
                    Radius = 180
                }
            }
        };
    }
}