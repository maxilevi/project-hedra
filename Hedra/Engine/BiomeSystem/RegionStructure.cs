using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.VillageSystem;

namespace Hedra.Engine.BiomeSystem
{
    public class RegionStructure
    {
        private readonly int _seed;
     
        public RegionStructure(int Seed, BiomeStructureDesign StructureDesign)
        {
            _seed = Seed;
            Designs = StructureDesign.Designs;
            VillageType = StructureDesign.VillageType;
        }

        public StructureDesign[] Designs { get; }
        public VillageType VillageType { get; }

        public static RegionStructure Interpolate(params RegionStructure[] Regions)
        {
            return Regions[0];
        }
    }
}
