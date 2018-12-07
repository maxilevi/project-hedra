using System;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using OpenTK;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class FarmParameters : IBuildingParameters
    {
        public FarmDesignTemplate Design { get; set; }
        public WindmillDesignTemplate WindmillDesign { get; set; }
        public PropTemplate PropDesign { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Random Rng { get; set; }
        public bool HasWindmill { get; set; }
        public DesignTemplate[] PropDesigns { get; set; }

        DesignTemplate IBuildingParameters.Design
        {
            get => Design;
            set => Design = value as FarmDesignTemplate;
        }

        public float GetSize(VillageCache Cache)
        {          
            var size = 0f;
            for (var i = 0; i < PropDesigns.Length; i++)
            {
                size = Math.Max(size, Cache.GrabSize(PropDesigns[i].Path).Xz.LengthFast * .75f);
            }
            return size;       
        }
    }
}