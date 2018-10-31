using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Game;
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

        protected abstract CollidableStructure Setup(Vector3 TargetPosition, Random Rng);
        
        protected virtual CollidableStructure Setup(Vector3 TargetPosition, Random Rng, BaseStructure Structure)
        {
            return new CollidableStructure(this, TargetPosition, new Plateau(TargetPosition, Radius), Structure);
        }

        public bool CanSetup(Vector3 TargetPosition)
        {
            return World.WorldBuilding.CanAddPlateau(new Plateau(TargetPosition, Radius));
        }

        public void CheckFor(Vector2 ChunkOffset, Region Biome, RandomDistribution Distribution)
        {
            for (var x = Math.Min(-2, -Radius / Chunk.Width * 2); x < Math.Max(2, Radius / Chunk.Width * 2); x++)
            {
                for (var z = Math.Min(-2, -Radius / Chunk.Width * 2); z < Math.Max(2, Radius / Chunk.Width * 2); z++)
                {
                    var offset = new Vector2(ChunkOffset.X + x * Chunk.Width,
                        ChunkOffset.Y + z * Chunk.Width);
                    Distribution.Seed = BiomeGenerator.GenerateSeed(offset);
                    var targetPosition = BuildTargetPosition(offset, Distribution);
                    var items = World.StructureHandler.StructureItems;
                    
                    if (this.ShouldSetup(offset, targetPosition, items, Biome, Distribution))
                    {
                        if (!CanSetup(targetPosition)) continue;
                        var item = this.Setup(targetPosition, BuildRng(offset));
                        World.StructureHandler.AddStructure(item);
                        World.StructureHandler.Build(item);
                    }
                }
            }
        }

        public bool ShouldRemove(Vector2 Offset, CollidableStructure Structure)
        {
            var width = Math.Max(2, Radius / Chunk.Width * 2) * 2 * Chunk.Width;
            return (Offset - Structure.Position.Xz).LengthFast > new Vector2(width, width).LengthFast;
        }

        public static Random BuildRng(Vector2 Offset)
        {
            return BiomeGenerator.GenerateRng(Offset);
        }
        
        public static int BuildRngSeed(Vector2 Offset)
        {
            return BiomeGenerator.GenerateSeed(Offset);
        }

        public static Vector3 BuildTargetPosition(Vector2 ChunkOffset, IRandom Rng)
        {
            return new Vector3(ChunkOffset.X + Rng.Next(0, (int)(Chunk.Width / Chunk.BlockSize)) * Chunk.BlockSize,
                0,
                ChunkOffset.Y + Rng.Next(0, (int)(Chunk.Width / Chunk.BlockSize)) * Chunk.BlockSize);
        }

        public bool ShouldSetup(Vector2 ChunkOffset, Vector3 TargetPosition, CollidableStructure[] Items, Region Biome, IRandom Rng)
        {
            bool shouldBe = this.SetupRequirements(TargetPosition, ChunkOffset, Biome, Rng)
                            && (TargetPosition.Xz - GameSettings.SpawnPoint).LengthSquared > 256 * 256;

            return shouldBe && this.ShouldBuild(TargetPosition, Items, Biome.Structures.Designs);
        }

        private bool ShouldBuild(Vector3 NewPosition, CollidableStructure[] Items, StructureDesign[] Designs)
        {
            float wSeed = World.Seed * 0.0001f;
            var height = (int) (World.StructureHandler.SeedGenerator.GetValue(NewPosition.X * .0085f + wSeed,
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

        protected abstract bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng);

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
