using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.PlantSystem
{
    public class BerryBushDesign : PlantDesign
    {
        public override VertexData Model => CacheManager.GetModel(CacheItem.BerryBush);
        public override Matrix4 TransMatrix(Vector3 Position, Random Rng)
        {
            var underChunk = World.GetChunkAt(Position);
            var blockPosition = World.ToBlockSpace(Position);
            var addon = new Vector3(Rng.NextFloat() * 16f, 0, Rng.NextFloat() * 16f);
            if (blockPosition.X + addon.X / Chunk.BlockSize > Chunk.ChunkWidth / Chunk.BlockSize) addon.X = 0;
            if (blockPosition.Z + addon.Z / Chunk.BlockSize > Chunk.ChunkWidth / Chunk.BlockSize) addon.Z = 0;

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
            Matrix4 transMatrix = Matrix4.CreateScale(1.75f + Rng.NextFloat() * .75f);
            transMatrix *= rotationMat4;
            transMatrix *= Matrix4.CreateTranslation(new Vector3(Position.X, height, Position.Z) + addon);
            return transMatrix;
        }

        public override VertexData Paint(Vector3 Position, VertexData Data, Random Rng)
        {
            Data = Data + CacheManager.GetModel(CacheItem.Berries).Clone();

            var underChunk = World.GetChunkAt(Position);
            Vector4 newColor = Utils.VariateColor(underChunk.Biome.Colors.GrassColor, 15, Rng);
            Vector4 berriesColor = Utils.VariateColor(Colors.BerryColor(Rng), 15, Rng);

            Data.Color(AssetManager.ColorCode0, newColor);
            Data.Color(AssetManager.ColorCode1, berriesColor);
            Data.GraduateColor(Vector3.UnitY);

            return Data;
        }

        public override void CustomPlacement(VertexData Data, Matrix4 TransMatrix)
        {
            var position = TransMatrix.ExtractTranslation();
            var underChunk = World.GetChunkAt(position);

            ThreadManager.ExecuteOnMainThread(delegate {
                var berryBush = new Entity
                {
                    Physics =
                    {
                        HasCollision = false,
                        UsePhysics = false,
                        HitboxSize = 0,
                        CanCollide = false
                    },
                    BlockPosition = position
                };
                berryBush.Model = new StaticModel(berryBush, Data)
                {
                    Position = position
                };

                var damage = new DamageComponent(berryBush)
                {
                    Immune = true
                };
                berryBush.AddComponent(damage);

                var berries = new BerryBushComponent(berryBush)
                {
                    UnderChunk = underChunk
                };
                berryBush.AddComponent(berries);

                World.AddEntity(berryBush);
            });
        }
    }
}
