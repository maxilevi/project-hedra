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
            Scheme = StructureDesign.Scheme;
        }

        public StructureDesign[] Designs { get; }
        public VillageScheme Scheme { get; }

        public static RegionStructure Interpolate(params RegionStructure[] Regions)
        {
            return Regions[0];
        }
    }
}
