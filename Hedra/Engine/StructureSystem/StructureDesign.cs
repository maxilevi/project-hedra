using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

        protected abstract CollidableStructure Setup(Vector3 TargetPosition, Vector2 NewOffset, Random Rng);

        public virtual void CheckFor(Vector2 ChunkOffset)
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

                    bool shouldBe = this.SetupRequirements(targetPosition, newOffset, rng) && (Math.Abs(targetPosition.X - 50000) > 2000 || Math.Abs(targetPosition.Y - 50000) > 2000);

                    if (shouldBe && !this.InOtherStrucutureRange(targetPosition))
                    {
                        lock(World.StructureGenerator.Items)
                            World.StructureGenerator.Items.Add(Setup(targetPosition, newOffset, rng));
                    }

                }
            }
        }

        private bool InOtherStrucutureRange(Vector3 NewPosition)
        {
            CollidableStructure[] items;
            lock (World.StructureGenerator.Items)
                items = World.StructureGenerator.Items.ToArray();


            return items.Any(Item =>
                (Item.Position.Xz - NewPosition.Xz).LengthSquared <
                (Item.Design.Radius + Radius) * (Item.Design.Radius + Radius));

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

        protected abstract bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Random Rng);

        public virtual bool MeetsRequirements(Vector2 ChunkOffset)
        {
            return World.Seed != World.MenuSeed;
        }
    }
}
