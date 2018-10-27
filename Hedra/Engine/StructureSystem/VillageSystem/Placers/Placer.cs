using System;
using System.Collections.Generic;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;

namespace Hedra.Engine.StructureSystem.VillageSystem.Placers
{
    public class Placer<T> where T : IBuildingParameters, new()
    {
        protected Random Rng { get; }
        protected DesignTemplate[] Designs { get; }
        
        public Placer(DesignTemplate[] Designs, Random Rng)
        {
            this.Designs = Designs;
            this.Rng = Rng;
        }

        public virtual T[] Place(PlacementPoint[] Points, float Chances)
        {
            var parameters = new List<T>();
            for (var i = 0; i < Points.Length; i++)
            {
                if (Rng.NextFloat() < Chances && this.SpecialRequirements(Points[i]))
                {
                    parameters.Add(this.FromPoint(Points[i]));
                }
            }
            return parameters.ToArray();
        }

        public virtual T FromPoint(PlacementPoint Point)
        {
            return new T
            {
                Design = this.SelectRandom(this.Designs),
                Position = Point.Position,
                Rng = Rng
            };
        }

        protected virtual bool SpecialRequirements(PlacementPoint Point)
        {
            return true;
        }
        
        protected T SelectRandom<T>(T[] Templates)
        {
            return Templates[Rng.Next(0, Templates.Length)];
        }
    }
}