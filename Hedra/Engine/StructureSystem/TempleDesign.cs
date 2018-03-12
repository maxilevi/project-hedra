using System;
using System.Collections.Generic;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.QuestSystem;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class TempleDesign : StructureDesign
    {
        public override int Radius { get; set; } = 700;

        public override void Build(Vector3 Position, CollidableStructure Structure)
        {
            var rng = new Random((int)(Position.X / 11 * (Position.Z / 13)));
            var model = AssetManager.PlyLoader("Assets/Env/IncaTemple0.ply", Vector3.One * 20f);
            var underChunk = World.GetChunkAt(Position);

            Matrix4 transMatrix = Matrix4.Identity;
            transMatrix *= Matrix4.CreateTranslation(Position);
            model.Transform(transMatrix);

            model.GraduateColor(Vector3.UnitY);

            /*List<CollisionShape> shapes = AssetManager.LoadCollisionShapes("GiantTree0.ply", 77, Vector3.One * 100f);
            for (int i = 0; i < shapes.Count; i++)
            {
                shapes[i].Transform(transMatrix);
                Structure.AddCollisionShape(shapes[i]);
            }*/

            ThreadManager.ExecuteOnMainThread(delegate
            {
                MobType BossType;
                Entity TreeBoss = BossGenerator.Generate(rng, out BossType);


                var prize = new Chest(Vector3.TransformPosition(Vector3.UnitZ * +10f + Vector3.UnitX * -80f, transMatrix), new Item.InventoryItem(Item.ItemType.Random));
                prize.Condition += () => TreeBoss == null || TreeBoss.IsDead;
                prize.Rotation = Vector3.UnitY * 90f;

                TreeBoss.Position = prize.Position.Xz.ToVector3() - Vector3.UnitZ * 30f;
                TreeBoss.Model.Position = prize.Position.Xz.ToVector3();
                TreeBoss.SearchComponent<BossAIComponent>().Protect = () => prize.Position;

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
            float height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out type);

            return false && Rng.Next(0, 25) == 1 && height > 0;
        }
    }
}
