using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EntitySystem.BossSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class GiantTreeDesign : StructureDesign
    {
        public override int Radius { get; set; } = 700;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.BossIcon);

        public override void Build(Vector3 Position, CollidableStructure Structure)
        {
            var rng = new Random((int)(Position.X / 11 * (Position.Z / 13)));
            var model = AssetManager.PlyLoader("Assets/Env/GiantTree0.ply", Vector3.One * 100f);
            var underChunk = World.GetChunkAt(Position);

            Matrix4 transMatrix = Matrix4.Identity;
            transMatrix *= Matrix4.CreateRotationY(rng.NextFloat() * 360);
            transMatrix *= Matrix4.CreateTranslation(Position + Vector3.UnitY * 7f);
            model.Transform(transMatrix);

            model.Color(AssetManager.ColorCode0, underChunk.Biome.Colors.WoodColor);
            model.Color(AssetManager.ColorCode1, underChunk.Biome.Colors.LeavesColor);
            model.Color(AssetManager.ColorCode2, underChunk.Biome.Colors.LeavesColor  * .8f);

            model.ExtraData.AddRange(model.GenerateWindValues());
            float treeRng = Utils.Rng.NextFloat();
            for (int i = 0; i < model.ExtraData.Count; i++)
            {
                model.ExtraData[i] = Mathf.Pack(new Vector2(model.ExtraData[i] * 2.5f, treeRng), 2048);
            }
            model.GraduateColor(Vector3.UnitY);

            List<CollisionShape> shapes = AssetManager.LoadCollisionShapes("GiantTree0.ply", 77, Vector3.One * 100f);
            for (int i = 0; i < shapes.Count; i++)
            {
                shapes[i].Transform(transMatrix);
                Structure.AddCollisionShape(shapes[i]);
            }

            ThreadManager.ExecuteOnMainThread(delegate
            {
                Entity treeBoss = BossGenerator.Generate(new [] { MobType.Gorilla, MobType.Troll }, rng);

                var prize = new Chest(Vector3.TransformPosition(Vector3.UnitZ * +10f + Vector3.UnitX * -80f, transMatrix),
                    ItemPool.Grab( new ItemPoolSettings(ItemTier.Uncommon) ));
                prize.Condition += () => treeBoss == null || treeBoss.IsDead;
                prize.Rotation = Vector3.UnitY * 90f;

                treeBoss.Position = prize.Position.Xz.ToVector3() - Vector3.UnitZ * 30f;
                treeBoss.Model.Position = prize.Position.Xz.ToVector3();
                //treeBoss.SearchComponent<BossAIComponent>().Protect = () => prize.Position;

                World.AddStructure(prize);
                underChunk.AddStaticElement(model);
            });

        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Vector2 NewOffset, Region Biome, Random Rng)
        {
            BlockType type;
            float height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out type);

            var plateau = new Plateau(TargetPosition, Radius, 800, height);

            World.QuestManager.AddPlateau(plateau);

            return new CollidableStructure(this, TargetPosition, plateau);
            
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, Random Rng)
        {
            BlockType type;
            float height = Biome.Generation.GetHeight( TargetPosition.X, TargetPosition.Z, null, out type);

            return Rng.Next(0, 100) == 1 && height > 0;
        }
    }
}
