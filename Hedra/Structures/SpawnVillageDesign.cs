using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Localization;
using Hedra.Numerics;

namespace Hedra.Structures
{
    public class SpawnVillageDesign : VillageDesign, IFindableStructureDesign
    {
        public override bool IsFixed => true;
        public static bool Spawned { get; set; }
        public string DisplayName => Translations.Get("quest_village");

        protected override Village BuildVillageObject(Vector3 TargetPosition)
        {
            return new SpawnVillage(TargetPosition);
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            Spawned = true;
            return base.Setup(TargetPosition, Rng);
        }

        public override bool ShouldSetup(Vector2 ChunkOffset, ref Vector3 TargetPosition, CollidableStructure[] Items,
            Region Biome, IRandom Rng)
        {
            return ChunkOffset == World.ToChunkSpace(World.SpawnVillagePoint) && !Spawned;
        }
    }
}