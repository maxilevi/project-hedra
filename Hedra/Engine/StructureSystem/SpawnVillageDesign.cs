using System;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class SpawnVillageDesign : VillageDesign
    {
        protected override Village BuildVillageObject(Vector3 TargetPosition)
        {
            return new SpawnVillage(TargetPosition);
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            World.StructureHandler.SpawnVillageSpawned = true;
            return base.Setup(TargetPosition, Rng);
        }

        public override bool ShouldSetup(Vector2 ChunkOffset, Vector3 TargetPosition, CollidableStructure[] Items, Region Biome, IRandom Rng)
        {
            return ChunkOffset == World.ToChunkSpace(World.SpawnVillagePoint) && !World.StructureHandler.SpawnVillageSpawned && false;
        }      
    }
}