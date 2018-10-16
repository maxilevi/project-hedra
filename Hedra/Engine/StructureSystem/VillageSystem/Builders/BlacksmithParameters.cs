using System;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class BlacksmithParameters : IBuildingParameters
    {
        public BlacksmithDesignTemplate Design { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Random Rng { get; set; }

        DesignTemplate IBuildingParameters.Design
        {
            get => Design;
            set => Design = value as BlacksmithDesignTemplate;
        }

        public float GetSize(VillageCache Cache)
        {
            return Cache.GrabSize(Design.Path).Xz.LengthFast * .5f;
        }
    }
}