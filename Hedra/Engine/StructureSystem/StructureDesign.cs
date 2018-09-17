using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public abstract class StructureDesign
    {
        public abstract int Radius { get; set; }
        public abstract VertexData Icon { get; }

        public abstract void Build(CollidableStructure Structure);

        protected virtual CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            var plateau = new Plateau(TargetPosition, Radius);
            World.WorldBuilding.AddPlateau(plateau);
            return new CollidableStructure(this, TargetPosition, plateau);
        }

        private bool CanSetup(Vector3 TargetPosition)
        {
            var plateau = new Plateau(TargetPosition, Radius);
            return World.WorldBuilding.CanAddPlateau(plateau);
        }

        public void CheckFor(Vector2 ChunkOffset, Region Biome)
        {
            for (var x = Math.Min(-2, -Radius / Chunk.Width * 2); x < Math.Max(2, Radius / Chunk.Width * 2); x++)
            {
                for (var z = Math.Min(-2, -Radius / Chunk.Width * 2); z < Math.Max(2, Radius / Chunk.Width * 2); z++)
                {
                    var offset = new Vector2(ChunkOffset.X + x * Chunk.Width,
                        ChunkOffset.Y + z * Chunk.Width);
                    var rng = this.BuildRng(offset);
                    var targetPosition = this.BuildTargetPosition(offset, rng);
                    var items = World.StructureGenerator.Structures;
                    
                    if (this.ShouldSetup(offset, targetPosition, items, Biome, rng))
                    {
                        if (!CanSetup(targetPosition)) continue;
                        var item = this.Setup(targetPosition, rng);
                        if (item == null) continue;
                        World.StructureGenerator.AddStructure(item);
                        World.StructureGenerator.Build(item);
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
                        if (Items[i].Design.GetType() == this.GetType() && (NewPosition.Xz - Items[i].Position.Xz).LengthFast < .05f)
                            return false;
                    }
                }
                return true;
            }
            return false;

        }

        protected abstract bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, Random Rng);

        public virtual bool MeetsRequirements(Vector2 ChunkOffset)
        {
            return World.Seed != World.MenuSeed;
        }

        public virtual void OnEnter(IPlayer Player)
        {         
        }

        public virtual int[] AmbientSongs { get; } = new int[0];
    }
}
