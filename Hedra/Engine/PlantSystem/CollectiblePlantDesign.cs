using System;
using Hedra.BiomeSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Generation;
using Hedra.Engine.StructureSystem;
using Hedra.Rendering;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.PlantSystem
{
    public class CollectiblePlantDesign : StructureDesign
    {
        public override int PlateauRadius 
            => throw new NotImplementedException();

        public override VertexData Icon
            => null;
        
        public override void Build(CollidableStructure Structure) 
            => throw new NotImplementedException();

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng) 
            => throw new NotImplementedException();

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng)
            => throw new NotImplementedException();
        
        public override bool ShouldRemove(CollidableStructure Structure)
        {
            return World.GetChunkByOffset(World.ToChunkSpace(Structure.Position)) == null;
        }
    }
}