using System;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class FarmParameters : IBuildingParameters
    {
        public FarmDesignTemplate Design { get; set; }
        public DesignTemplate WindmillDesign { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Random Rng { get; set; }
        public bool HasWindmill { get; set; }

        DesignTemplate IBuildingParameters.Design
        {
            get => Design;
            set => Design = value as FarmDesignTemplate;
        }
        
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

        public float GetSize(VillageCache Cache)
        {
            var diameter = Cache.GrabSize(Design.Path).Xz.LengthFast * 3f;
            return diameter * .5f;
        }
    }
}