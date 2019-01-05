using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public class BerryBushDesign : PlantDesign
    {
        public override CacheItem Type => CacheItem.BerryBush;
        
        public override bool HasCustomPlacement => true;

        protected virtual float Scale(Random Rng) => 1.75f + Rng.NextFloat() * .75f;
        
        public override Matrix4 TransMatrix(Vector3 Position, Random Rng)
        {
            var underChunk = World.GetChunkAt(Position);
            var blockPosition = World.ToBlockSpace(Position);
            var addon = new Vector3(Rng.NextFloat() * 16f, 0, Rng.NextFloat() * 16f);
            if (blockPosition.X + addon.X / Chunk.BlockSize > Chunk.Width / Chunk.BlockSize) addon.X = 0;
            if (blockPosition.Z + addon.Z / Chunk.BlockSize > Chunk.Width / Chunk.BlockSize) addon.Z = 0;

            float height = Physics.HeightAtPosition(Position + addon);
            var topBlock = World.GetHighestBlockAt((int)(Position.X + addon.X), (int)(Position.Z + addon.Z));
            if (Block.Noise3D) return Matrix4.Identity;

            for (int x = -3; x < 3; x++)
            {
                for (int z = -3; z < 3; z++)
                {
                    float bDens = Physics.HeightAtPosition(new Vector3((blockPosition.X + x) * Chunk.BlockSize + underChunk.OffsetX, 0, (blockPosition.Z + z) * Chunk.BlockSize + underChunk.OffsetZ));
                    float difference = Math.Abs(bDens - height);
                    if (difference > 5f) return Matrix4.Identity;
                }
            }

            var rotationMat4 = Matrix4.CreateRotationY(360 * Utils.Rng.NextFloat() * Mathf.Radian);
            var transMatrix = Matrix4.CreateScale(Scale(Rng));
            transMatrix *= rotationMat4;
            transMatrix *= Matrix4.CreateTranslation(new Vector3(Position.X, height, Position.Z) + addon);
            return transMatrix;
        }

        public override VertexData Paint(VertexData Data, Region Region, Random Rng)
        {
            Data.Color(AssetManager.ColorCode0, Region.Colors.GrassColor);
            Data.Color(AssetManager.ColorCode1, Colors.BerryColor(Rng));
            
            return Data;
        }

        public override void CustomPlacement(VertexData Data, Matrix4 TransMatrix, Chunk UnderChunk)
        {
            var position = Vector3.TransformPosition(Vector3.Zero, TransMatrix);
            World.StructureHandler.AddStructure(
                new CollidableStructure(
                    new CollectiblePlantDesign(),
                    position,
                    null,
                    new CollectiblePlant(
                        position,
                        Data.ToInstanceData(TransMatrix),
                        ItemCollect
                    )
                )
                {
                    Built = true
                }
            );
        }

        protected virtual Item ItemCollect => ItemPool.Grab(ItemType.Berry);
    }
}
