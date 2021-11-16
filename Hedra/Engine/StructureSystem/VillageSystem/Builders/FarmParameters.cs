using System;
using System.Numerics;
using Hedra.Engine.StructureSystem.VillageSystem.Templates;
using Hedra.Numerics;

namespace Hedra.Engine.StructureSystem.VillageSystem.Builders
{
    public class FarmParameters : IBuildingParameters
    {
        public FarmDesignTemplate Design { get; set; }
        public WindmillDesignTemplate WindmillDesign { get; set; }
        public PropTemplate PropDesign { get; set; }
        public bool HasWindmill { get; set; }
        public bool InsidePaths { get; set; }
        public float BonusFarmHeight { get; set; } = .35f;
        public DesignTemplate[] PropDesigns { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Random Rng { get; set; }

        DesignTemplate IBuildingParameters.Design
        {
            get => Design;
            set => Design = value as FarmDesignTemplate;
        }

        public float GetSize(VillageCache Cache)
        {
            var size = 0f;
            for (var i = 0; i < PropDesigns.Length; i++)
                size = Math.Max(size, Cache.GrabSize(PropDesigns[i].Path).Xz().LengthFast() * .75f);
            return size;
        }
    }
}