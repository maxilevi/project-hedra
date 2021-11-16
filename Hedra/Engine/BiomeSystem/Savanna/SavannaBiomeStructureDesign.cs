using Hedra.BiomeSystem;
using Hedra.Engine.StructureSystem.VillageSystem;

namespace Hedra.Engine.BiomeSystem.Savanna
{
    public class SavannaBiomeStructureDesign : BiomeStructureDesign
    {
        public override VillageType VillageType => VillageType.Woodland; // Should be Desert
    }
}