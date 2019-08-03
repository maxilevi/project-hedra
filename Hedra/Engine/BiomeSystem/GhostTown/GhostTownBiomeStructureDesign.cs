using System;
using Hedra.BiomeSystem;
using Hedra.Engine.StructureSystem.GhostTown;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Structures;

namespace Hedra.Engine.BiomeSystem.GhostTown
{
    public class GhostTownBiomeStructureDesign : BiomeStructureDesign
    {
        public override VillageType VillageType => VillageType.None;

        public GhostTownBiomeStructureDesign()
        {
            AddDesign(new TombStructureDesign());
            AddDesign(new SpawnGhostTownPortalDesign());
            AddDesign(new GhostTownBossDesign());
        }
    }
}