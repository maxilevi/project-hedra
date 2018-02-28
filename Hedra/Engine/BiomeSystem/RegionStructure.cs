using Hedra.Engine.StructureSystem;

namespace Hedra.Engine.BiomeSystem
{
    public class RegionStructure
    {
        private readonly int _seed;
     
        public RegionStructure(int Seed, BiomeStructureDesign StructureDesign)
        {
            _seed = Seed;
            Designs = StructureDesign.Designs;
        }

        public StructureDesign[] Designs { get; }

        public static RegionStructure Interpolate(params RegionStructure[] Regions)
        {
            return Regions[0];
        }
    }
}
