using System;
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
    public class RockDesign :  PlantDesign
    {
        public override VertexData Model => CacheManager.GetModel(CacheItem.Rock);
        public override Matrix4 TransMatrix(Vector3 Position, Random Rng)
        {
            var underChunk = World.GetChunkAt(Position);
            var blockPosition = World.ToBlockSpace(Position);
            var addon = new Vector3(Rng.NextFloat() * 16f, 0, Rng.NextFloat() * 16f) * Chunk.BlockSize;
            if (blockPosition.X + addon.X / Chunk.BlockSize > Chunk.Width / Chunk.BlockSize) addon.X = 0;
            if (blockPosition.Z + addon.Z / Chunk.BlockSize > Chunk.Width / Chunk.BlockSize) addon.Z = 0;

            float height = Physics.HeightAtPosition(Position + addon);
            Block topBlock = World.GetHighestBlockAt((int)(Position.X + addon.X), (int)(Position.Z + addon.Z));
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
            Matrix4 transMatrix = Matrix4.CreateScale(2.75f + Rng.NextFloat() * .75f);
            transMatrix *= rotationMat4;
            transMatrix *= Matrix4.CreateTranslation(new Vector3(Position.X, height, Position.Z) + addon);
            return transMatrix;
        }

        public override VertexData Paint(Vector3 Position, VertexData Data, Random Rng)
        {
            Data.ExtraData.AddRange(Data.GenerateWindValues());
            for (int i = 0; i < Data.ExtraData.Count; i++)
                Data.ExtraData[i] = 0.001f;

            Data.ReColor(this.RockColor(Rng));
            Data.GraduateColor(Vector3.UnitY);

            return Data; 
        }

        public override void AddShapes(Chunk UnderChunk, Matrix4 TransMatrix)
        {
            List<CollisionShape> newShapes = CacheManager.GetShape(Model);
            newShapes.ForEach(Shape => Shape.Transform(TransMatrix));

            UnderChunk.AddCollisionShape(newShapes.ToArray());
        }

        private Vector4 RockColor(Random Rng)
        {
            switch (Rng.Next(0, 2))
            {
                case 0: return Colors.FromHtml("#976548");
                default: return Colors.Gray;
            }
        }
    }
}
