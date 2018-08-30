using System;
using System.Collections.Generic;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
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
    public class GiantTreeDesign : StructureDesign
    {
        public override int Radius { get; set; } = 700;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.BossIcon);

        public override void Build(CollidableStructure Structure)
        {
            var position = Structure.Position;
            var region = World.BiomePool.GetRegion(position);
            var rng = new Random((int)(position.X / 11 * (position.Z / 13)));
            var originalModel = CacheManager.GetModel(CacheItem.GiantTree);
            var model = originalModel.ShallowClone();

            Matrix4 transMatrix = Matrix4.CreateScale(Vector3.One * 100f);
            transMatrix *= Matrix4.CreateRotationY(rng.NextFloat() * 360);
            transMatrix *= Matrix4.CreateTranslation(position + Vector3.UnitY * 7f);
            model.Transform(transMatrix);

            model.Color(AssetManager.ColorCode0, region.Colors.WoodColor);
            model.Color(AssetManager.ColorCode1, region.Colors.LeavesColor);
            model.Color(AssetManager.ColorCode2, region.Colors.LeavesColor  * .8f);

            model.Extradata.AddRange(model.GenerateWindValues());
            float treeRng = Utils.Rng.NextFloat();
            for (int i = 0; i < model.Extradata.Count; i++)
            {
                model.Extradata[i] = Mathf.Pack(new Vector2(model.Extradata[i] * 2.5f, treeRng), 2048);
            }
            model.GraduateColor(Vector3.UnitY);

            List<CollisionShape> shapes = CacheManager.GetShape(originalModel).DeepClone();
            for (int i = 0; i < shapes.Count; i++)
            {
                shapes[i].Transform(transMatrix);
            }

            Executer.ExecuteOnMainThread(delegate
            {
                Entity treeBoss = BossGenerator.Generate(new [] { MobType.Beetle, MobType.Gorilla }, rng);

                var prize = new Chest(Vector3.TransformPosition(Vector3.UnitZ * +10f + Vector3.UnitX * -80f, transMatrix),
                    ItemPool.Grab( new ItemPoolSettings(ItemTier.Uncommon) ));
                prize.Condition += () => treeBoss == null || treeBoss.IsDead;
                prize.Rotation = Vector3.UnitY * 90f;

                treeBoss.Position = prize.Position.Xz.ToVector3() - Vector3.UnitZ * 30f;
                treeBoss.Model.Position = prize.Position.Xz.ToVector3();
                //treeBoss.SearchComponent<BossAIComponent>().Protect = () => prize.Position;

                World.AddStructure(prize);
            });
            Structure.AddCollisionShape(shapes.ToArray());
            Structure.AddStaticElement(model);
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
            float height = Biome.Generation.GetHeight( TargetPosition.X, TargetPosition.Z, null, out type);

            return Rng.Next(0, 100) == 1 && height > 0;
        }
    }
}
