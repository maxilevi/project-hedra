using System;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;

namespace Hedra.Engine.StructureSystem.VillageSystem.Placers
{
    public class Placer<T> : IPlacer<T> where T : IBuildingParameters, new()
    {
        public Placer(DesignTemplate[] Designs, Random Rng)
        {
            this.Designs = Designs;
            this.Rng = Rng;
        }

        protected Random Rng { get; }
        protected DesignTemplate[] Designs { get; }

        public virtual T Place(PlacementPoint Point)
        {
            if (SpecialRequirements(Point)) return FromPoint(Point);
            throw new ArgumentOutOfRangeException("Could not place object");
        }

        public virtual bool SpecialRequirements(PlacementPoint Point)
        {
            return true;
        }

        protected virtual T FromPoint(PlacementPoint Point)
        {
            return new T
            {
                Design = SelectRandom(Designs),
                Position = Point.Position,
                Rng = Rng
            };
        }

        protected T SelectRandom<T>(T[] Templates) where T : class
        {
            if (Templates.Length == 0) return null;
            return Templates[Rng.Next(0, Templates.Length)];
        }
    }
}