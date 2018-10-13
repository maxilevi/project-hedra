using System;
using System.Collections.Generic;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EntitySystem.BossSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class TempleDesign : StructureDesign
    {
        public override int Radius { get; set; } = 700;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.BossIcon);

        public override void Build(CollidableStructure Structure)
        {
            var position = Structure.Position;
            var rng = new Random((int)(position.X / 11 * (position.Z / 13)));
            var model = AssetManager.PLYLoader("Assets/Env/IncaTemple0.ply", Vector3.One * 20f);

            Matrix4 transMatrix = Matrix4.Identity;
            transMatrix *= Matrix4.CreateTranslation(position);
            model.Transform(transMatrix);

            model.GraduateColor(Vector3.UnitY);

            /*List<CollisionShape> shapes = AssetManager.LoadCollisionShapes("GiantTree0.ply", 77, Vector3.One * 100f);
            for (int i = 0; i < shapes.Count; i++)
            {
                shapes[i].Transform(transMatrix);
                Structure.AddCollisionShape(shapes[i]);
            }*/

            Executer.ExecuteOnMainThread(delegate
            {
                //var treeBoss = BossGenerator.Generate(new [] {MobType.Gorilla, MobType.Troll}, rng);
/*
                var prize = new Chest(Vector3.TransformPosition(Vector3.UnitZ * +10f + Vector3.UnitX * -80f, transMatrix),
                    ItemPool.Grab( new ItemPoolSettings(ItemTier.Uncommon) ));
                prize.Condition += () => treeBoss == null || treeBoss.IsDead;
                prize.Rotation = Vector3.UnitY * 90f;

                treeBoss.Position = prize.Position.Xz.ToVector3() - Vector3.UnitZ * 30f;
                treeBoss.Model.Position = prize.Position.Xz.ToVector3();*/
                //treeBoss.SearchComponent<BossAIComponent>().Protect = () => prize.Position;

                //World.AddStructure(prize);
               // underChunk.AddStaticElement(model);
            });
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng)
        {
            float height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out _);

            return false && Rng.Next(0, 25) == 1 && height > 0;
        }
    }
}
