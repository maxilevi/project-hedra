namespace Hedra.Engine.StructureSystem.VillageSystem
{
    public class VillageRing
    {
        public float Radius { get; set; }
        public VillageRing InnerRing { get; set; }

        public static VillageRing Default => new VillageRing
        {
            Radius = 1024,
            InnerRing = new VillageRing
            {
                Radius = 768,
                InnerRing = new VillageRing
                {
                    Radius = 256
                }
            }
        };

        public bool Collides(PlacementPoint Point)
        {
            return Collide(this, Point);
        }

        public static bool Collide(VillageRing Ring, PlacementPoint Point)
        {
            return Point.Position.LengthSquared() < Ring.Radius * Ring.Radius &&
                   (Ring.InnerRing == null || !Ring.InnerRing.Collides(Point));
        }
    }
}