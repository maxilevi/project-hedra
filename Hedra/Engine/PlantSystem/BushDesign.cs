using System;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    internal class BushDesign : PlantDesign
    {
        public override VertexData Model => CacheManager.GetModel(CacheItem.Bushes);
        public override Matrix4 TransMatrix(Vector3 Position, Random Rng)
        {
            var underChunk = World.GetChunkAt(Position);
            var blockPosition = World.ToBlockSpace(Position);
            var addon = new Vector3(Rng.NextFloat() * 16f, 0, Rng.NextFloat() * 16f);
            if (blockPosition.X + addon.X / Chunk.BlockSize > Chunk.Width / Chunk.BlockSize) addon.X = 0;
            if (blockPosition.Z + addon.Z / Chunk.BlockSize > Chunk.Width / Chunk.BlockSize) addon.Z = 0;

            float height = Physics.HeightAtPosition(Position + addon);
            var topBlock = World.GetHighestBlockAt((int)(Position.X + addon.X), (int)(Position.Z + addon.Z));
            if (topBlock.Noise3D) return Matrix4.Identity;

            for (int x = -3; x < 3; x++)
            {
                for (int z = -3; z < 3; z++)
                {
                    float bDens = Physics.HeightAtPosition(new Vector3((blockPosition.X + x) * Chunk.BlockSize + underChunk.OffsetX, 0, (blockPosition.Z + z) * Chunk.BlockSize + underChunk.OffsetZ));
                    float difference = Math.Abs(bDens - height);
                    if (difference > 5f) return Matrix4.Identity;
                }
            }

            Matrix4 rotationMat4 = Matrix4.CreateRotationY(360 * Utils.Rng.NextFloat());
            Matrix4 transMatrix = Matrix4.CreateScale(1.75f + Rng.NextFloat() * .75f);
            transMatrix *= rotationMat4;
            transMatrix *= Matrix4.CreateTranslation(new Vector3(Position.X, height, Position.Z) + addon);
            return transMatrix;
        }

        public override VertexData Paint(Vector3 Position, VertexData Data, Random Rng)
        {
            Data.ExtraData.AddRange(Data.GenerateWindValues());

            var underchunk = World.GetChunkAt(Position);
            Data.ReColor(underchunk.Biome.Colors.GrassColor * .8f);
            Data.GraduateColor(Vector3.UnitY);

            return Data;
        }
    }
}
