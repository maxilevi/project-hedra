using System;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    internal class FarmParameters : IBuildingParameters
    {
        public DesignTemplate Design { get; set; }
        public DesignTemplate WindmillDesign { get; set; }
        public Vector3 Position { get; set; }
        public Random Rng { get; set; }
        public bool HasWindmill { get; set; }

        public FarmParameters AlterPosition(Vector3 Offset)
        {
            return new FarmParameters
            {
                Design = Design,
                WindmillDesign = WindmillDesign,
                Position = Position + Offset,
                Rng = Rng,
                HasWindmill = HasWindmill
            };
        }
    }
}