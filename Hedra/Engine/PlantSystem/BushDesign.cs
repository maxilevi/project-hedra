using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Numerics;

namespace Hedra.Engine.PlantSystem
{
    public class BushDesign : PlantDesign
    {
        public override CacheItem Type => CacheItem.Bushes;

        public override Matrix4x4 TransMatrix(Vector3 Position, Random Rng)
        {
            var underChunk = World.GetChunkAt(Position);
            if (underChunk == null) return new Matrix4x4();
            var blockPosition = World.ToBlockSpace(Position);
            var addon = new Vector3(Rng.NextFloat() * 16f, 0, Rng.NextFloat() * 16f);
            if (blockPosition.X + addon.X / Chunk.BlockSize > Chunk.Width / Chunk.BlockSize) addon.X = 0;
            if (blockPosition.Z + addon.Z / Chunk.BlockSize > Chunk.Width / Chunk.BlockSize) addon.Z = 0;

            var height = Physics.HeightAtPosition(Position + addon);
            var topBlock = World.GetHighestBlockAt((int)(Position.X + addon.X), (int)(Position.Z + addon.Z));
            if (Block.Noise3D) return new Matrix4x4();

            for (var x = -3; x < 3; x++)
            for (var z = -3; z < 3; z++)
            {
                var bDens = Physics.HeightAtPosition(new Vector3(
                    (blockPosition.X + x) * Chunk.BlockSize + underChunk.OffsetX, 0,
                    (blockPosition.Z + z) * Chunk.BlockSize + underChunk.OffsetZ));
                var difference = Math.Abs(bDens - height);
                if (difference > 5f) return new Matrix4x4();
            }

            var rotationMat4 = Matrix4x4.CreateRotationY(360 * Utils.Rng.NextFloat() * Mathf.Radian);
            var transMatrix = Matrix4x4.CreateScale(1.75f + Rng.NextFloat() * .75f);
            transMatrix *= rotationMat4;
            transMatrix *= Matrix4x4.CreateTranslation(new Vector3(Position.X, height, Position.Z) + addon);
            return transMatrix;
        }

        public override NativeVertexData Paint(NativeVertexData Data, Region Region, Random Rng)
        {
            Data.AddWindValues();

            Data.Paint(Region.Colors.GrassColor * .8f);
            Data.GraduateColor(Vector3.UnitY);

            return Data;
        }
    }
}