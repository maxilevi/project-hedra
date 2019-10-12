using System;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class BuildingParameters : IBuildingParameters
    {
        public DesignTemplate Design { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        
        public Random Rng { get; set; }

        public virtual float GetSize(VillageCache Cache)
        {
            return Cache.GrabSize(Design.Path).Xz.LengthFast * .5f;
        }
    }
}