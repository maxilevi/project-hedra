using Hedra.BiomeSystem;
using Hedra.Engine.StructureSystem.GhostTown;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Structures;

namespace Hedra.Engine.BiomeSystem.GhostTown
{
    public class GhostTownBiomeStructureDesign : BiomeStructureDesign
    {
        public GhostTownBiomeStructureDesign()
        {
            AddDesign(new TombStructureDesign());
            AddDesign(new SpawnGhostTownPortalDesign());
            AddDesign(new GhostTownBossDesign());
        }

        public override VillageType VillageType => VillageType.None;
    }
}