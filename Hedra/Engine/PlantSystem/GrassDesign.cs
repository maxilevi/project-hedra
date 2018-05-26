﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public class GrassDesign : PlantDesign
    {
        public override VertexData Model => CacheManager.GetModel(CacheItem.Grass);
        public override Matrix4 TransMatrix(Vector3 Position, Random Rng)
        {
            var underChunk = World.GetChunkAt(Position);
            var blockPosition = World.ToBlockSpace(Position);
            var addon = new Vector3(Rng.NextFloat() * 2f, 0, Rng.NextFloat() * 2f);
            if (blockPosition.X + addon.X / Chunk.BlockSize > Chunk.Width / Chunk.BlockSize) addon.X = 0;
            if (blockPosition.Z + addon.Z / Chunk.BlockSize > Chunk.Width / Chunk.BlockSize) addon.Z = 0;

            float height = Physics.HeightAtPosition(Position + addon);
            Block topBlock = World.GetHighestBlockAt((int)(Position.X + addon.X), (int)(Position.Z + addon.Z));
            if (topBlock.Noise3D) return Matrix4.Identity;

            for (int x = -1; x < 1; x++)
            {
                if (x == 0) continue;
                float blockDens = Physics.HeightAtPosition(new Vector3((blockPosition.X + x) * Chunk.BlockSize + underChunk.OffsetX, 0, (blockPosition.Z + 0) * Chunk.BlockSize + underChunk.OffsetZ));
                float difference = Math.Abs(blockDens - height);
                if (difference > 6)
                    return Matrix4.Identity;
            }

            for (int z = -1; z < 1; z++)
            {
                if (z == 0) continue;
                float blockDens = Physics.HeightAtPosition(new Vector3((blockPosition.X + 0) * Chunk.BlockSize + underChunk.OffsetX, 0, (blockPosition.Z + z) * Chunk.BlockSize + underChunk.OffsetZ));
                float difference = Math.Abs(blockDens - height);
                if (difference > 6)
                    return Matrix4.Identity;
            }
            Matrix4 rotationMat4 = Matrix4.CreateRotationY(360 * Utils.Rng.NextFloat());
            Matrix4 transMatrix = Matrix4.CreateScale(6.0f + Utils.Rng.NextFloat() * .5f);
            transMatrix *= rotationMat4;
            transMatrix *= Matrix4.CreateTranslation(new Vector3(Position.X, height, Position.Z) + addon);
            return transMatrix;
        }

        public override VertexData Paint(Vector3 Position, VertexData Data, Random Rng)
        {
            var region = World.BiomePool.GetRegion(Position);
            var newColor = new Vector4((region.Colors.GrassColor * 1.0f).Xyz, 1);

            Data.ReColor(newColor);
            Vector3 highest = Model.SupportPoint(Vector3.UnitY);
            Vector3 lowest = Model.SupportPoint(-Vector3.UnitY);
            float dot = Vector3.Dot(highest - lowest, Vector3.UnitY);

            for (var i = 0; i < Data.Vertices.Count; i++)
            {
                float shade = Vector3.Dot(Data.Vertices[i] - lowest, Vector3.UnitY) / dot;
                Data.Colors[i] += new Vector4(.3f, .3f, .3f, 0) * shade;
            }
            return Data;
        }
    }
}
