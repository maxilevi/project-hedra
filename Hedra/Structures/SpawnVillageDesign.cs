using System;
using Hedra.BiomeSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Generation;
using Hedra.Engine.StructureSystem;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Localization;
using System.Numerics;

namespace Hedra.Structures
{
    public class SpawnVillageDesign : VillageDesign, IFindableStructureDesign
    {
        public static bool Spawned { get; set; }
        
        protected override Village BuildVillageObject(Vector3 TargetPosition)
        {
            return new SpawnVillage(TargetPosition);
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            Spawned = true;
            return base.Setup(TargetPosition, Rng);
        }

        public override bool ShouldSetup(Vector2 ChunkOffset, Vector3 TargetPosition, CollidableStructure[] Items, Region Biome, IRandom Rng)
        {
            return ChunkOffset == World.ToChunkSpace(World.SpawnVillagePoint) && !Spawned;
        }
        public string DisplayName => Translations.Get("quest_village");
    }
}