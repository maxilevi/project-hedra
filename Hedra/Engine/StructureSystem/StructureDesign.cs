using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Player.MapSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.StructureSystem
{
    public abstract class StructureDesign
    {
        public virtual int SearchRadius => PlateauRadius;
        public abstract int PlateauRadius { get; }
        public abstract VertexData Icon { get; }
        public abstract void Build(CollidableStructure Structure);

        protected abstract CollidableStructure Setup(Vector3 TargetPosition, Random Rng);
        
        protected CollidableStructure Setup(Vector3 TargetPosition, Random Rng, BaseStructure Structure)
        {
            var plateau = new RoundedPlateau(TargetPosition.Xz(), PlateauRadius);
            plateau.MaxHeight = World.WorldBuilding.ApplyMultiple(plateau.Position, plateau.MaxHeight);
            var collidable = new CollidableStructure(this, new Vector3(TargetPosition.X, plateau.MaxHeight, TargetPosition.Z), plateau, Structure);
            Structure.Position = collidable.Position;
            return collidable;
        }
   
        protected static Random BuildRng(CollidableStructure Structure)
        {
            return new Random((int) (Structure.Position.X / 11 * (Structure.Position.Z / 13)));
        }
        
        public void CheckFor(Vector2 ChunkOffset, Region Biome, RandomDistribution Distribution)
        {
            for (var x = Math.Min(-2, -SearchRadius / Chunk.Width * 2); x < Math.Max(2, SearchRadius / Chunk.Width * 2); x++)
            {
                for (var z = Math.Min(-2, -SearchRadius / Chunk.Width * 2); z < Math.Max(2, SearchRadius / Chunk.Width * 2); z++)
                {
                    var offset = new Vector2(ChunkOffset.X + x * Chunk.Width,
                        ChunkOffset.Y + z * Chunk.Width);
                    Distribution.Seed = BuildRngSeed(offset);
                    var targetPosition = BuildTargetPosition(offset, Distribution);
                    var items = World.StructureHandler.StructureItems;
                    
                    if (this.ShouldSetup(offset, ref targetPosition, items, Biome, Distribution) && !World.StructureHandler.StructureExistsAtPosition(targetPosition))
                    {
                        World.StructureHandler.RegisterStructure(targetPosition);
                        var item = this.Setup(targetPosition, BuildRng(offset));
                        item.MapPosition = offset;
                        World.StructureHandler.AddStructure(item);
                        World.StructureHandler.Build(item);
                    }
                }
            }
        }

        public virtual bool ShouldRemove(CollidableStructure Structure)
        {
            var chunkOffset = World.ToChunkSpace(Structure.Position);
            for (var x = Math.Min(-2, -SearchRadius / Chunk.Width * 2); x < Math.Max(2, SearchRadius / Chunk.Width * 2); x++)
            {
                for (var z = Math.Min(-2, -SearchRadius / Chunk.Width * 2); z < Math.Max(2, SearchRadius / Chunk.Width * 2); z++)
                {
                    var offset = new Vector2(chunkOffset.X + x * Chunk.Width, chunkOffset.Y + z * Chunk.Width);
                    if (World.GetChunkByOffset(offset) != null)
                        return false;
                }
            }

            return true;
        }

        public static Random BuildRng(Vector2 Offset)
        {
            return BiomeGenerator.GenerateRng(Offset);
        }
        
        public static int BuildRngSeed(Vector2 Offset)
        {
            return Unique.GenerateSeed(Offset);
        }

        public static Vector3 BuildTargetPosition(Vector2 ChunkOffset, IRandom Rng)
        {
            return new Vector3(ChunkOffset.X + Rng.Next(0, (int)(Chunk.Width / Chunk.BlockSize)) * Chunk.BlockSize,
                0,
                ChunkOffset.Y + Rng.Next(0, (int)(Chunk.Width / Chunk.BlockSize)) * Chunk.BlockSize);
        }

        public virtual bool ShouldSetup(Vector2 ChunkOffset, ref Vector3 TargetPosition, CollidableStructure[] Items, Region Biome, IRandom Rng)
        {
            var shouldBe = this.SetupRequirements(ref TargetPosition, ChunkOffset, Biome, Rng);
            if (World.BiomePool.Type == WorldType.Overworld)
            {
                shouldBe &= (TargetPosition - World.SpawnPoint).Xz().LengthSquared() > 512 * 512;
                shouldBe &= (TargetPosition - World.SpawnVillagePoint).Xz().LengthSquared() >
                            VillageDesign.MaxVillageRadius * 1.5f * VillageDesign.MaxVillageRadius * 1.5f;
            }
            return shouldBe && this.ShouldBuild(TargetPosition, Items, Biome.Structures.Designs);
        }

        protected virtual bool ShouldBuild(Vector3 NewPosition, CollidableStructure[] Items, StructureDesign[] Designs)
        {
            return StructureGrid.Sample(World.ToChunkSpace(NewPosition), this, Designs) && !AlreadySpawnedStructure(NewPosition, Items);
        }

        private bool AlreadySpawnedStructure(Vector3 NewPosition, CollidableStructure[] Items)
        {
            lock (Items)
            {
                for (var i = 0; i < Items.Length; i++)
                {
                    if (Items[i].Design.GetType() == this.GetType() && (NewPosition.Xz() - Items[i].Position.Xz()).LengthFast() < .05f)
                        return true;
                }
            }
            return false;
        }
        
        protected abstract bool SetupRequirements(ref Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng);

        protected static void DoWhenChunkReady(Vector3 Position, Action<Vector3> Do, CollidableStructure Structure)
        {
            DecorationsPlacer.PlaceWhenWorldReady(Position, Do, () => Structure.Disposed);
        }
        
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
