using System;
using System.Drawing.Drawing2D;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.PlantSystem
{
    public abstract class WeedDesign : PlantDesign
    {
        public override Matrix4x4 TransMatrix(Vector3 Position, Random Rng)
        {
            var underChunk = World.GetChunkAt(Position);
            var blockPosition = World.ToBlockSpace(Position);
            var addon = new Vector3(Rng.NextFloat() * 2f, 0, Rng.NextFloat() * 2f);
            if (blockPosition.X + addon.X / Chunk.BlockSize > Chunk.Width / Chunk.BlockSize) return new Matrix4x4();
            if (blockPosition.Z + addon.Z / Chunk.BlockSize > Chunk.Width / Chunk.BlockSize) return new Matrix4x4();

            float height = Physics.HeightAtPosition(Position + addon);
            var topBlock = World.GetHighestBlockAt((int)(Position.X + addon.X), (int)(Position.Z + addon.Z));

            for (int x = -1; x < 1; x++)
            {
                if (x == 0) continue;
                float blockDens = Physics.HeightAtPosition(new Vector3((blockPosition.X + x) * Chunk.BlockSize + underChunk.OffsetX, 0, (blockPosition.Z + 0) * Chunk.BlockSize + underChunk.OffsetZ));
                float difference = Math.Abs(blockDens - height);
                if (difference > 6)
                    return new Matrix4x4();
            }

            for (int z = -1; z < 1; z++)
            {
                if (z == 0) continue;
                float blockDens = Physics.HeightAtPosition(new Vector3((blockPosition.X + 0) * Chunk.BlockSize + underChunk.OffsetX, 0, (blockPosition.Z + z) * Chunk.BlockSize + underChunk.OffsetZ));
                float difference = Math.Abs(blockDens - height);
                if (difference > 6)
                    return new Matrix4x4();
            }
            Matrix4x4 rotationMat4 = Matrix4x4.CreateRotationY(360 * Utils.Rng.NextFloat() * Mathf.Radian);
            Matrix4x4 transMatrix = Matrix4x4.CreateScale(6.0f + Utils.Rng.NextFloat() * .5f);
            transMatrix *= rotationMat4;
            transMatrix *= Matrix4x4.CreateTranslation(new Vector3(Position.X, height, Position.Z) + addon);
            return transMatrix;
        }
        
        public override NativeVertexData Paint(NativeVertexData Data, Region Region, Random Rng)
        {
            ApplyPaint(Data, Region, Rng);
            
            var highest = Model.SupportPoint(Vector3.UnitY);
            var lowest = Model.SupportPoint(-Vector3.UnitY);
            var dot = Vector3.Dot(highest - lowest, Vector3.UnitY);

            for (var i = 0; i < Data.Vertices.Count; i++)
            {
                float shade = Vector3.Dot(Data.Vertices[i] - lowest, Vector3.UnitY) / dot;
                Data.Colors[i] += new Vector4(.3f, .3f, .3f, 0) * shade;
            }
            return Data;
        }

        protected abstract void ApplyPaint(NativeVertexData Data, Region Region, Random Rng);
    }
}