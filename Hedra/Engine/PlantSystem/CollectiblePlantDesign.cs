using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.StructureSystem;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.PlantSystem
{
    public class CollectiblePlantDesign : StructureDesign
    {
        public override int StructureChance => throw new NotImplementedException();
        public override int PlateauRadius
            => throw new NotImplementedException();

        public override VertexData Icon
            => null;

        public override bool CanSpawnInside => throw new NotImplementedException();

        public override void Build(CollidableStructure Structure)
        {
            throw new NotImplementedException();
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            throw new NotImplementedException();
        }

        protected override bool SetupRequirements(ref Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome,
            IRandom Rng)
        {
            throw new NotImplementedException();
        }

        public override bool ShouldRemove(CollidableStructure Structure)
        {
            return World.GetChunkByOffset(World.ToChunkSpace(Structure.Position)) == null;
        }
    }
}