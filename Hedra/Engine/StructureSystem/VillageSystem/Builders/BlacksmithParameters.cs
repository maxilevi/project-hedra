using System;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    internal class BlacksmithParameters : IBuildingParameters
    {
        public BlacksmithDesignTemplate Design { get; set; }
        public Vector3 Position { get; set; }
        public Random Rng { get; set; }

        DesignTemplate IBuildingParameters.Design
        {
            get => Design;
            set => Design = value as BlacksmithDesignTemplate;
        }
    }
}