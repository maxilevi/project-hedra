using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public abstract class StructureDesign
    {
        public bool SpawnInMenu { get; set; }
        public abstract int Radius { get; set; }
        public abstract VertexData Icon { get; }

        public abstract void Build(Vector3 Position, CollidableStructure Structure);

        protected abstract CollidableStructure Setup(Vector3 TargetPosition, Vector2 NewOffset, Region Biome, Random Rng);

        public void CheckFor(Vector2 ChunkOffset, Region Biome)
        {
            for (int x = Math.Min(-2, -Radius / Chunk.Width * 2); x < Math.Max(2, Radius / Chunk.Width * 2); x++)
            {
                for (int z = Math.Min(-2, -Radius / Chunk.Width * 2); z < Math.Max(2, Radius / Chunk.Width * 2); z++)
                {
                    var offset = new Vector2(ChunkOffset.X + x * Chunk.Width,
                        ChunkOffset.Y + z * Chunk.Width);
                    var rng = this.BuildRng(offset);
                    var targetPosition = this.BuildTargetPosition(offset, rng);
                    CollidableStructure[] items;
                    lock (World.StructureGenerator.Items)
                        items = World.StructureGenerator.Items.ToArray();
                    
                    if (this.ShouldSetup(offset, targetPosition, items, Biome, rng))
                    {
                        lock (World.StructureGenerator.Items)
                        {
                            var item = this.Setup(targetPosition, offset, Biome, rng);
                            if(item != null) World.StructureGenerator.Items.Add(item);
                        }
                    }
                }
            }
        }

        public Random BuildRng(Vector2 Offset)
        {
            return BiomeGenerator.GenerateRng(Offset);
        }

        public Vector3 BuildTargetPosition(Vector2 ChunkOffset, Random Rng)
        {
            return new Vector3(ChunkOffset.X + Rng.Next(0, (int)(Chunk.Width / Chunk.BlockSize)) * Chunk.BlockSize,
                0,
                ChunkOffset.Y + Rng.Next(0, (int)(Chunk.Width / Chunk.BlockSize)) * Chunk.BlockSize);
        }

        public bool ShouldSetup(Vector2 ChunkOffset, Vector3 TargetPosition, CollidableStructure[] Items, Region Biome, Random Rng)
        {
            bool shouldBe = this.SetupRequirements(TargetPosition, ChunkOffset, Biome, Rng)
                            && (TargetPosition.Xz - GameSettings.SpawnPoint).LengthSquared > 256 * 256;

            return shouldBe && this.ShouldBuild(TargetPosition, Items, Biome.Structures.Designs);
        }

        private bool ShouldBuild(Vector3 NewPosition, CollidableStructure[] Items, StructureDesign[] Designs)
        {
            float wSeed = World.Seed * 0.0001f;
            var height = (int) (World.StructureGenerator.SeedGenerator.GetValue(NewPosition.X * .0085f + wSeed,
                          NewPosition.Z * .0085f + wSeed) * 100f);
            var index = new Random(height).Next(0, Designs.Length);
            bool isStructureRegion = index == Array.IndexOf(Designs, this);
            if (isStructureRegion)
            {
                lock (Items)
                {
                    for (var i = 0; i < Items.Length; i++)
                    {
                        if (Items[i].Design.GetType() == this.GetType() && NewPosition == Items[i].Position)
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

        }

        protected abstract bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, Random Rng);

        public virtual bool MeetsRequirements(Vector2 ChunkOffset)
        {
            return World.Seed != World.MenuSeed;
        }
    }
}
