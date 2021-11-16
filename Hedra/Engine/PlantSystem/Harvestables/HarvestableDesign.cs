using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Numerics;

namespace Hedra.Engine.PlantSystem.Harvestables
{
    public abstract class HarvestableDesign : PlantDesign
    {
        public override bool HasCustomPlacement => true;

        protected abstract Item ItemCollect { get; }

        public virtual float Scale(Random Rng)
        {
            return 1.75f + Rng.NextFloat() * .75f;
        }

        public override Matrix4x4 TransMatrix(Vector3 Position, Random Rng)
        {
            var underChunk = World.GetChunkAt(Position);
            var blockPosition = World.ToBlockSpace(Position);
            var addon = new Vector3(Rng.NextFloat() * 16f, 0, Rng.NextFloat() * 16f);
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
            var transMatrix = Matrix4x4.CreateScale(Scale(Rng));
            transMatrix *= rotationMat4;
            transMatrix *= Matrix4x4.CreateTranslation(new Vector3(Position.X, height, Position.Z) + addon);
            return transMatrix;
        }

        public override NativeVertexData Paint(NativeVertexData Data, Region Region, Random Rng)
        {
            Data.Color(AssetManager.ColorCode0, Region.Colors.GrassColor);
            return Data;
        }

        public override void CustomPlacement(NativeVertexData Data, Matrix4x4 TransMatrix, Chunk UnderChunk)
        {
            var position = Vector3.Transform(Vector3.Zero, TransMatrix);
            if (!World.StructureHandler.StructureExistsAtPosition(position.Xz().ToVector3()))
            {
                var collectibleObject = new CollectibleObject(
                    position,
                    Data.ToInstanceData(TransMatrix),
                    ItemCollect
                );
                var design = new CollectiblePlantDesign();
                World.StructureHandler.AddStructure(
                    new CollidableStructure(
                        design,
                        position,
                        null,
                        collectibleObject
                    )
                    {
                        Built = true
                    }
                );
            }
            else
            {
                Data.Dispose();
            }
        }
    }
}