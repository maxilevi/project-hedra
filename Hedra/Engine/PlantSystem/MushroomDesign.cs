using System;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public class MushroomDesign : PlantDesign
    {
        public override VertexData Model => CacheManager.GetModel(CacheItem.Mushroom);
        public override bool HasCustomPlacement => true;

        public override Matrix4 TransMatrix(Vector3 Position, Random Rng)
        {
            var underChunk = World.GetChunkAt(Position);
            var blockPosition = World.ToBlockSpace(Position);
            var addon = new Vector3(Rng.NextFloat() * 16f, 0, Rng.NextFloat() * 16f);
            if (blockPosition.X + addon.X / Chunk.BlockSize > Chunk.Width / Chunk.BlockSize) addon.X = 0;
            if (blockPosition.Z + addon.Z / Chunk.BlockSize > Chunk.Width / Chunk.BlockSize) addon.Z = 0;

            float height = Physics.HeightAtPosition(Position + addon);
            var topBlock = World.GetHighestBlockAt((int) (Position.X + addon.X), (int) (Position.Z + addon.Z));
            if (topBlock.Noise3D) return Matrix4.Identity;

            for (int x = -3; x < 3; x++)
            {
                for (int z = -3; z < 3; z++)
                {
                    float bDens = Physics.HeightAtPosition(new Vector3(
                        (blockPosition.X + x) * Chunk.BlockSize + underChunk.OffsetX, 0,
                        (blockPosition.Z + z) * Chunk.BlockSize + underChunk.OffsetZ));
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

        public override VertexData Paint(Vector3 Position, VertexData Data, Region Region, Random Rng)
        {
            Vector4 stemColor = Utils.VariateColor(Colors.MushroomStemColor(Rng), 15, Rng);
            Vector4 headColor = Utils.VariateColor(Colors.MushroomHeadColor(Rng), 15, Rng);

            Data.Color(AssetManager.ColorCode0, stemColor);
            Data.Color(AssetManager.ColorCode1, headColor);

            return Data;
        }

        public override void CustomPlacement(VertexData Data, Matrix4 TransMatrix)
        {
            var position = TransMatrix.ExtractTranslation();
            TransMatrix = TransMatrix.ClearTranslation();
            Data.Transform(TransMatrix);

            Executer.ExecuteOnMainThread(delegate
            {
                var mushroom = new Entity
                {
                    Physics =
                    {
                        HasCollision = false,
                        UsePhysics = false,
                        CanCollide = false
                    },
                    BlockPosition = position
                };
                mushroom.Model = new StaticModel(mushroom, Data)
                {
                    Position = position
                };

                var damage = new DamageComponent(mushroom)
                {
                    Immune = true
                };
                mushroom.AddComponent(damage);
                mushroom.AddComponent(
                    new CollectibleComponent(mushroom, ItemPool.Grab(ItemType.Mushroom), "You pickep up a mushroom")
                );
                World.AddEntity(mushroom);
            });
        }
    }
}