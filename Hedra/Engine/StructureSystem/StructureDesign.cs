using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public abstract class StructureDesign
    {
        public bool SpawnInMenu { get; set; }
        public abstract int Radius { get; set; }

        public abstract void Build(Vector3 Position, CollidableStructure Structure);

        protected abstract CollidableStructure Setup(Vector3 TargetPosition, Vector2 NewOffset, Region Biome, Random Rng);

        public virtual void CheckFor(Vector2 ChunkOffset, Region Biome)
        {
            for (int x = Math.Min(-2, -Radius / Chunk.ChunkWidth * 2); x < Math.Max(2, Radius / Chunk.ChunkWidth * 2); x++)
            {
                for (int z = Math.Min(-2, -Radius / Chunk.ChunkWidth * 2); z < Math.Max(2, Radius / Chunk.ChunkWidth * 2); z++)
                {

                    var newOffset = new Vector2(ChunkOffset.X + x * Chunk.ChunkWidth, ChunkOffset.Y + z * Chunk.ChunkWidth);
                    Random rng = BiomeGenerator.GenerateRng(newOffset);

                    var targetPosition = new Vector3(newOffset.X + rng.Next(0, (int)(Chunk.ChunkWidth / Chunk.BlockSize)) * Chunk.BlockSize,
                        0,
                        newOffset.Y + rng.Next(0, (int)(Chunk.ChunkWidth / Chunk.BlockSize)) * Chunk.BlockSize);
                    bool shouldBe = this.SetupRequirements(targetPosition, newOffset, Biome, rng)
                        && (targetPosition.Xz - GameSettings.SpawnPoint).LengthSquared > 256*256;
                    if (shouldBe && this.ShouldBuild(targetPosition, Biome.Structures.Designs))
                    {
                        lock (World.StructureGenerator.Items)
                        {
                            var item = Setup(targetPosition, newOffset, Biome, rng);
                            if(item != null) World.StructureGenerator.Items.Add(item);
                        }
                    }
                }
            }
        }

        private bool ShouldBuild(Vector3 NewPosition, StructureDesign[] Designs)
        {
            float wSeed = World.Seed * 0.0001f;
            var height = (int) (World.StructureGenerator.SeedGenerator.GetValue(NewPosition.X * .01f + wSeed,
                          NewPosition.Z * .01f + wSeed) * 100f);
            var index = new Random(height).Next(0, Designs.Length);
            bool isStructureRegion = index == Array.IndexOf(Designs, this);
            if (isStructureRegion)
            {
                lock (World.StructureGenerator.Items)
                {
                    for (var i = 0; i < World.StructureGenerator.Items.Count; i++)
                    {
                        if (World.StructureGenerator.Items[i].Design.GetType() == this.GetType() && NewPosition == World.StructureGenerator.Items[i].Position)
                            return false;
                    }
                }
                return true;
            }
            return false;

        }

        protected IEnumerator BuildOnChunk(object[] Params)
        {
            var position = (Vector3)Params[0];
            var model = Params[1] as VertexData;
            var boxes = Params[2] as List<CollisionShape>;

            Chunk underChunk = World.GetChunkAt(position);
            int currentSeed = World.Seed;
            while ((underChunk == null || !underChunk.BuildedWithStructures || !underChunk.Initialized) && World.Seed == currentSeed)
            {
                underChunk = World.GetChunkAt(position);
                yield return null;
            }

            if (underChunk == null) yield break;

            if (boxes != null)
                underChunk.AddCollisionShape(boxes.ToArray());

            if (model != null)
                underChunk.AddStaticElement(model);

            underChunk.Blocked = true;
            World.AddChunkToQueue(underChunk, true);

        }

        protected abstract bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, Random Rng);

        public virtual bool MeetsRequirements(Vector2 ChunkOffset)
        {
            return World.Seed != World.MenuSeed;
        }
    }
}
