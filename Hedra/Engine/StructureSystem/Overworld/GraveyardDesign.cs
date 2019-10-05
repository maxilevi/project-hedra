using System;
using System.Collections.Generic;
using System.Drawing;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.WorldBuilding;
using Hedra.Items;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.Sound;
using OpenTK;
using Region = Hedra.BiomeSystem.Region;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class GraveyardDesign : CompletableStructureDesign<Graveyard>
    {
        private const int Level = 9;
        public override int PlateauRadius { get; } = 384;
        public const int GraveyardSkyTime = 24000;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.GraveyardIcon);

        public override void Build(CollidableStructure Structure)
        {
            var position = Structure.Position;
            var rng = new Random( (int) ( position.X / 11 * (position.Z / 13) ) );

            const int tombstoneCount = 25;
            
            var rotationMatrix = Matrix4.CreateRotationY(rng.NextFloat() * 360 * Mathf.Radian);
            var originalMausoleum = CacheManager.GetModel(CacheItem.Mausoleum);
            var mausoleum = originalMausoleum.ShallowClone();

            mausoleum.Transform(Matrix4.CreateScale(4f));
            mausoleum.Transform(rotationMatrix);
            mausoleum.Translate(position);
            mausoleum.GraduateColor(Vector3.UnitY);

            var mausoleumShapes = CacheManager.GetShape(originalMausoleum).DeepClone();
            for (var i = 0; i < mausoleumShapes.Count; i++)
            {
                mausoleumShapes[i].Transform(Matrix4.CreateScale(4f));
                mausoleumShapes[i].Transform(rotationMatrix);
                mausoleumShapes[i].Transform(position);
            }

            int k = 0, j = 0;
            for (var i = 0; i < tombstoneCount * 2; i++)
            {

                if (i % 2 == 0) continue;

                if (j >= 5)
                {
                    j = 0;
                    k++;
                }

                j++;
                if (rng.Next(0, 4) == 1 || (j == 3 && k == 2)) continue;

                var gravePosition = position + Vector3.UnitX * 28f * Chunk.BlockSize + Vector3.UnitZ * 18f * Chunk.BlockSize
                    + Vector3.UnitX * -11 * j * Chunk.BlockSize
                    + Vector3.UnitZ * -11 * k * Chunk.BlockSize;
                gravePosition = new Vector3(gravePosition.X, position.Y, gravePosition.Z);

                var graveScale = Vector3.One * (3.25f + rng.NextFloat() * .5f) * 1.5f;
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

                Structure.AddCollisionShape(shapes.ToArray());
                Structure.AddStaticElement(grave);
                if (rng.Next(0, 5) == 1)
                {
                    Structure.WorldObject.AddChildren(new Tombstone(gravePosition));
                }
            }
            for (var i = 0; i < mausoleum.Colors.Count; i++)
                mausoleum.Colors[i] *= new Vector4(.75f, .75f, .75f, 1);

            Structure.AddCollisionShape(mausoleumShapes.ToArray());
            Structure.AddStaticElement(mausoleum);
            ((Graveyard) Structure.WorldObject).AreaWrapper =
                World.HighlightArea(position, new Vector4(.1f, .1f, .1f, 1f), PlateauRadius, -1);

            this.BuildLamps(position, Structure.WorldObject, Structure);
            BuildReward(position, (Graveyard) Structure.WorldObject, rng);
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            return base.Setup(TargetPosition, Rng, new Graveyard(TargetPosition, PlateauRadius));
        }

        private static void BuildReward(Vector3 Position, Graveyard Cementery, Random Rng)
        {
            var enemies = new List<Entity>();

            var skeletonCount = 4;
            for (var i = 0; i < skeletonCount; i++)
            {
                var skeleton = World.WorldBuilding.SpawnBandit(
                        Position + new Vector3(Rng.NextFloat() * 60f - 30f, 0, Rng.NextFloat() * 60f - 30f) * Chunk.BlockSize,
                        Level, false, true);
                    enemies.Add(skeleton);
            }
            Cementery.Enemies = enemies.ToArray();

            var prize = World.SpawnChest(Position + Vector3.UnitX * 40f, 
                    ItemPool.Grab( new ItemPoolSettings(ItemTier.Uncommon) ));
            Cementery.Chest = prize;
            prize.Condition = delegate
            {
                if (!Cementery.Completed)
                {
                    LocalPlayer.Instance.MessageDispatcher.ShowNotification(Translations.Get("enemies_around"), Color.DarkRed, 2f);
                }
                return Cementery.Completed;
            };
            Cementery.AddChildren(prize);
        }

        private void BuildLamps(Vector3 Position, BaseStructure Cementery, CollidableStructure Structure)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3 addonPosition = Vector3.Zero;

                if (i == 0) addonPosition = Vector3.UnitZ * 14 * Chunk.BlockSize + Vector3.UnitX * 14 * Chunk.BlockSize;
                if (i == 1) addonPosition = -Vector3.UnitZ * 14 * Chunk.BlockSize - Vector3.UnitX * 14 * Chunk.BlockSize;
                if (i == 2) addonPosition = -Vector3.UnitZ * 14 * Chunk.BlockSize + Vector3.UnitX * 14 * Chunk.BlockSize;
                if (i == 3) addonPosition = Vector3.UnitZ * 14 * Chunk.BlockSize - Vector3.UnitX * 14 * Chunk.BlockSize;

                Vector3 lightPosition = Position + addonPosition;
                VertexData lampPost = AssetManager.PLYLoader("Assets/Env/Village/Lamp0.ply", Vector3.One * 3.25f * 1.5f);
                lampPost.Translate(lightPosition);
                lampPost.GraduateColor(Vector3.UnitY);
                lampPost.FillExtraData(WorldRenderer.NoHighlightFlag);
                List<CollisionShape> shapes = AssetManager.LoadCollisionShapes("Assets/Env/Village/Lamp0.ply", 1, Vector3.One * 3.25f * 1.5f);
                for (int l = 0; l < shapes.Count; l++)
                {
                    shapes[l].Transform(lightPosition);
                }
                Structure.AddCollisionShape(shapes.ToArray());
                Structure.AddStaticElement(lampPost);
                
                var lamp = new WorldLight(lightPosition + Vector3.UnitY * 7)
                {
                    Radius = PointLight.DefaultRadius,
                    LightColor = new Vector3(1, .6f, .5f)
                };
                Cementery.AddChildren(lamp);
            }
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng)
        {
            var height = Biome.Generation.GetMaxHeight(TargetPosition.X, TargetPosition.Z);
            return Rng.Next(0, StructureGrid.GraveyardChance) == 1 && height > BiomePool.SeaLevel;
        }
        
        public override int[] AmbientSongs => new []
        {
            SoundtrackManager.GraveyardChampion
        };
        
        public override string DisplayName => Translations.Get("structure_graveyard");

        protected override string GetShortDescription(Graveyard Structure)
        {
            return Translations.Get("quest_complete_structure_short_graveyard", DisplayName);
        }

        protected override string GetDescription(Graveyard Structure)
        {
            return Translations.Get("quest_complete_structure_description_graveyard", DisplayName, Structure.EnemiesLeft);
        }
    }
}
