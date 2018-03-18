using System;
using System.Collections.Generic;
using System.Drawing;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering;
using OpenTK;
using Region = Hedra.Engine.BiomeSystem.Region;

namespace Hedra.Engine.StructureSystem
{
    public class GraveyardDesign : StructureDesign
    {
        public override int Radius { get; set; } = 384;
        public const int GraveyardSkyTime = 24000;

        public override void Build(Vector3 Position, CollidableStructure Structure)
        {
            Chunk underChunk = World.GetChunkAt(Position);

            var rng = new Random( (int) ( Position.X / 11 * (Position.Z / 13) ) );

            const int tombstoneCount = 25;
            const int graveTypes = 6;
            var cementery = new Graveyard(Position, Radius);

            World.HighlightArea(Position, new Vector4(.2f, .2f, .2f, 1f) * .5f, Radius * 1.75f, -1);

            Matrix4 rotationMatrix = Matrix4.CreateRotationY(rng.NextFloat() * 360 * Mathf.Radian);
            VertexData mausoleum = AssetManager.PlyLoader("Assets/Env/Mausoleum.ply", Vector3.One * 4f);

            mausoleum.Transform(rotationMatrix);
            mausoleum.Transform(Position);
            mausoleum.GraduateColor(Vector3.UnitY);

            List<CollisionShape> mausoleumShapes = AssetManager.LoadCollisionShapes("Assets/Env/Mausoleum.ply", 2, Vector3.One * 4f);
            for (int i = 0; i < mausoleumShapes.Count; i++)
            {
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

                int graveType = rng.Next(0, graveTypes);
                Vector3 graveScale = Vector3.One * (3.25f + rng.NextFloat() * .5f) * 1.5f;
                VertexData grave = AssetManager.PlyLoader("Assets/Env/Grave" + graveType + ".ply", graveScale);
                grave.GraduateColor(Vector3.UnitY);
                grave.Transform(gravePosition);

                List<CollisionShape> shapes = AssetManager.LoadCollisionShapes("Assets/Env/Grave" + graveType + ".ply", 1, graveScale);
                for (int l = 0; l < shapes.Count; l++)
                {
                    shapes[l].Transform(gravePosition);
                }

                CoroutineManager.StartCoroutine(BuildOnChunk, new object[] { gravePosition, grave, shapes });

            }
            for (int i = 0; i < mausoleum.Colors.Count; i++)
                mausoleum.Colors[i] *= new Vector4(.75f, .75f, .75f, 1);

            underChunk.AddCollisionShape(mausoleumShapes.ToArray());
            underChunk.AddStaticElement(mausoleum);

            this.BuildLamps(Position);
            this.BuildReward(Position, cementery, rng);

            underChunk.Blocked = true;
            World.AddChunkToQueue(underChunk, true);
        }

        private void BuildReward(Vector3 Position, Graveyard Cementery, Random Rng)
        {
            var enemies = new List<Entity>();

            //execute this last

            var skeletonCount = 4;
            for (int i = 0; i < skeletonCount; i++)
            {
                int _k = i;
                ThreadManager.ExecuteOnMainThread(delegate
                {
                    Humanoid skeleton = World.QuestManager.SpawnBandit(
                        Position + new Vector3(Rng.NextFloat() * 60f - 30f, 0, Rng.NextFloat() * 60f - 30f) * Chunk.BlockSize,
                        false, true);

                    if (_k == 0)
                    {
                        //UsecustomNPC
                        //skeleton.MaxHealth *= 3f;
                        skeleton.Health = skeleton.MaxHealth;
                        skeleton.Model.Resize(Vector3.One * 2.5f);
                        skeleton.SearchComponent<HealthBarComponent>().DistanceFromBase *= 3.5f;
                    }
                    else
                    {
                        //UsecustomNPC
                        //skeleton.MaxHealth *= 1.5f;
                        skeleton.Health = skeleton.MaxHealth;
                        skeleton.Model.Resize(Vector3.One * 1.5f);
                        skeleton.SearchComponent<HealthBarComponent>().DistanceFromBase *= 2.5f;
                    }
                    enemies.Add(skeleton);

                });
            }

            //Chest
            Chest prize;
            ThreadManager.ExecuteOnMainThread(delegate
            {
                prize = World.QuestManager.SpawnChest(Position + Vector3.UnitX * 40f, 
                    ItemPool.Grab( new ItemPoolSettings(ItemTier.Uncommon) ));
                prize.Condition = delegate
                {
                    if (!Cementery.Restored)
                    {
                        LocalPlayer.Instance.MessageDispatcher.ShowNotification("THERE ARE STILL ENEMIES AROUND.", Color.DarkRed, 2f);
                    }
                    return Cementery.Restored;
                };
            });

            ThreadManager.ExecuteOnMainThread(delegate
            {
                Cementery.Enemies = enemies.ToArray();
            });
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
                VertexData lampPost = AssetManager.PlyLoader("Assets/Env/Lamp0.ply", Vector3.One * 3.25f * 1.5f);
                lampPost.Transform(lightPosition);
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

            BlockType type;
            float height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out type) + Chunk.BlockSize;

            var plateau = new Plateau(TargetPosition, Radius, 580, height);
            World.QuestManager.AddPlateau(plateau);

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
