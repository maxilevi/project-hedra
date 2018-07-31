﻿using System;
using System.Collections.Generic;
using System.Drawing;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.Rendering;
using OpenTK;
using Region = Hedra.Engine.BiomeSystem.Region;

namespace Hedra.Engine.StructureSystem
{
    public class GraveyardDesign : StructureDesign
    {
        public override int Radius { get; set; } = 384;
        public const int GraveyardSkyTime = 24000;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.GraveyardIcon);

        public override void Build(Vector3 Position, CollidableStructure Structure)
        {
            Chunk underChunk = World.GetChunkAt(Position);

            var rng = new Random( (int) ( Position.X / 11 * (Position.Z / 13) ) );

            const int tombstoneCount = 25;
            var cementery = new Graveyard(Position, Radius);

            World.HighlightArea(Position, new Vector4(.2f, .2f, .2f, 1f) * .5f, Radius * 1.75f, -1);

            var rotationMatrix = Matrix4.CreateRotationY(rng.NextFloat() * 360 * Mathf.Radian);
            var originalMausoleum = CacheManager.GetModel(CacheItem.Mausoleum);
            var mausoleum = originalMausoleum.ShallowClone();

            mausoleum.Transform(Matrix4.CreateScale(4f));
            mausoleum.Transform(rotationMatrix);
            mausoleum.Translate(Position);
            mausoleum.GraduateColor(Vector3.UnitY);

            var mausoleumShapes = CacheManager.GetShape(originalMausoleum).DeepClone();
            for (var i = 0; i < mausoleumShapes.Count; i++)
            {
                mausoleumShapes[i].Transform(Matrix4.CreateScale(4f));
                mausoleumShapes[i].Transform(rotationMatrix);
                mausoleumShapes[i].Transform(Position);
            }

            World.AddStructure(cementery);


            int k = 0, j = 0;
            for (int i = 0; i < tombstoneCount * 2; i++)
            {

                if (i % 2 == 0) continue;

                if (j >= 5)
                {
                    j = 0;
                    k++;
                }

                j++;
                if (rng.Next(0, 4) == 1 || (j == 3 && k == 2)) continue;

                Vector3 gravePosition = Position + Vector3.UnitX * 28f * Chunk.BlockSize + Vector3.UnitZ * 18f * Chunk.BlockSize
                    + Vector3.UnitX * -11 * j * Chunk.BlockSize
                    + Vector3.UnitZ * -11 * k * Chunk.BlockSize;
                gravePosition = new Vector3(gravePosition.X, Position.Y, gravePosition.Z);

                Vector3 graveScale = Vector3.One * (3.25f + rng.NextFloat() * .5f) * 1.5f;
                var originalGrave = CacheManager.GetModel(CacheItem.Grave);
                var grave = originalGrave.ShallowClone();
                grave.Scale(graveScale);
                grave.GraduateColor(Vector3.UnitY);
                grave.Translate(gravePosition);

                var shapes = CacheManager.GetShape(originalGrave).DeepClone();
                for (var l = 0; l < shapes.Count; l++)
                {
                    shapes[l].Transform(Matrix4.CreateScale(graveScale));
                    shapes[l].Transform(gravePosition);
                }

                CoroutineManager.StartCoroutine(this.BuildOnChunk, gravePosition, grave, shapes);
                if (rng.Next(0, 5) == 1)
                {
                    World.AddStructure(new Tombstone
                    {
                        Position = gravePosition
                    });
                }
            }
            for (var i = 0; i < mausoleum.Colors.Count; i++)
                mausoleum.Colors[i] *= new Vector4(.75f, .75f, .75f, 1);

            underChunk.AddCollisionShape(mausoleumShapes.ToArray());
            underChunk.AddStaticElement(mausoleum);

            this.BuildLamps(Position);
            BuildReward(Position, cementery, rng);

            underChunk.Blocked = true;
        }

        private static void BuildReward(Vector3 Position, Graveyard Cementery, Random Rng)
        {
            var enemies = new List<Entity>();

            var skeletonCount = 4;
            for (var i = 0; i < skeletonCount; i++)
            {
                var skeleton = World.WorldBuilding.SpawnBandit(
                        Position + new Vector3(Rng.NextFloat() * 60f - 30f, 0, Rng.NextFloat() * 60f - 30f) * Chunk.BlockSize,
                        false, true);
                    enemies.Add(skeleton);
            }
            Cementery.Enemies = enemies.ToArray();

            var prize = World.WorldBuilding.SpawnChest(Position + Vector3.UnitX * 40f, 
                    ItemPool.Grab( new ItemPoolSettings(ItemTier.Uncommon) ));
            prize.Condition = delegate
            {
                if (!Cementery.Restored)
                {
                    LocalPlayer.Instance.MessageDispatcher.ShowNotification("THERE ARE STILL ENEMIES AROUND.", Color.DarkRed, 2f);
                }
                return Cementery.Restored;
            };
        }

        private void BuildLamps(Vector3 Position)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3 addonPosition = Vector3.Zero;

                if (i == 0) addonPosition = Vector3.UnitZ * 14 * Chunk.BlockSize + Vector3.UnitX * 14 * Chunk.BlockSize;
                if (i == 1) addonPosition = -Vector3.UnitZ * 14 * Chunk.BlockSize - Vector3.UnitX * 14 * Chunk.BlockSize;
                if (i == 2) addonPosition = -Vector3.UnitZ * 14 * Chunk.BlockSize + Vector3.UnitX * 14 * Chunk.BlockSize;
                if (i == 3) addonPosition = Vector3.UnitZ * 14 * Chunk.BlockSize - Vector3.UnitX * 14 * Chunk.BlockSize;

                Vector3 lightPosition = Position + addonPosition;
                VertexData lampPost = AssetManager.PLYLoader("Assets/Env/Lamp0.ply", Vector3.One * 3.25f * 1.5f);
                lampPost.Translate(lightPosition);
                lampPost.GraduateColor(Vector3.UnitY);
                lampPost.FillExtraData(WorldRenderer.NoHighlightFlag);
                List<CollisionShape> shapes = AssetManager.LoadCollisionShapes("Assets/Env/Lamp0.ply", 1, Vector3.One * 3.25f * 1.5f);
                for (int l = 0; l < shapes.Count; l++)
                {
                    shapes[l].Transform(lightPosition);
                }

                CoroutineManager.StartCoroutine(BuildOnChunk, new object[] { lightPosition, lampPost, shapes });

                var Lamp = new LampPost(lightPosition + Vector3.UnitY * 7)
                {
                    Radius = 120f,
                    LightColor = new Vector3(1, .6f, .5f)
                };

                World.AddStructure(Lamp);
            }
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Vector2 NewOffset, Region Biome, Random Rng)
        {
            var plateau = new Plateau(TargetPosition, Radius);
            World.WorldBuilding.AddPlateau(plateau);

            return new CollidableStructure(this, TargetPosition, plateau);
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, Random Rng)
        {
            BlockType type;
            float height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out type);
            return Rng.Next(0, 75) == 1 && height > 0;
        }
    }
}
