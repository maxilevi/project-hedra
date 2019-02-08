using System;
using System.Collections.Generic;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;

namespace Hedra.Engine.StructureSystem.VillageSystem.Placers
{
    public class Placer<T> : IPlacer<T> where T : IBuildingParameters, new()
    {
        protected Random Rng { get; }
        protected DesignTemplate[] Designs { get; }
        
        public Placer(DesignTemplate[] Designs, Random Rng)
        {
            this.Designs = Designs;
            this.Rng = Rng;
        }

        public virtual T Place(PlacementPoint Point)
        {
            if (this.SpecialRequirements(Point))
            {
                return this.FromPoint(Point);
            }
            throw new ArgumentOutOfRangeException("Could not place object");
        }

        protected virtual T FromPoint(PlacementPoint Point)
        {
            return new T
            {
                Design = this.SelectRandom(this.Designs),
                Position = Point.Position,
                Rng = Rng
            };
        }

        public virtual bool SpecialRequirements(PlacementPoint Point)
        {
            return true;
        }
        
        protected T SelectRandom<T>(T[] Templates) where T : class
        {
            if (Templates.Length == 0) return null;
            return Templates[Rng.Next(0, Templates.Length)];
        }
    }
}