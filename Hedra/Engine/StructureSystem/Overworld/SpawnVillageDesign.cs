using System;
using Hedra.BiomeSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Generation;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class SpawnVillageDesign : VillageDesign
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
    }
}