using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.PlantSystem
{
    public class RockDesign : PlantDesign
    {
        public override CacheItem Type => CacheItem.Rock;

        public override Matrix4x4 TransMatrix(Vector3 Position, Random Rng)
        {
            var underChunk = World.GetChunkAt(Position);
            var blockPosition = World.ToBlockSpace(Position);
            var addon = new Vector3(Rng.NextFloat() * 16f, 0, Rng.NextFloat() * 16f) * Chunk.BlockSize;
            if (blockPosition.X + addon.X / Chunk.BlockSize > Chunk.Width / Chunk.BlockSize) addon.X = 0;
            if (blockPosition.Z + addon.Z / Chunk.BlockSize > Chunk.Width / Chunk.BlockSize) addon.Z = 0;

            var height = Physics.HeightAtPosition(Position + addon);
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
            var transMatrix = Matrix4x4.CreateScale(2.75f + Rng.NextFloat() * .75f);
            transMatrix *= rotationMat4;
            transMatrix *= Matrix4x4.CreateTranslation(new Vector3(Position.X, height, Position.Z) + addon);
            return transMatrix;
        }

        public override NativeVertexData Paint(NativeVertexData Data, Region Region, Random Rng)
        {
            Data.Paint(RockColor(Rng));
            Data.GraduateColor(Vector3.UnitY);
            return Data;
        }

        public override void AddShapes(Chunk UnderChunk, Matrix4x4 TransMatrix)
        {
            var newShapes = CacheManager.GetShape(Model).DeepClone();
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