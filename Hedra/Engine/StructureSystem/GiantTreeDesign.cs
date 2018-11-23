using System;
using System.Collections.Generic;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EntitySystem.BossSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Rendering;
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

            Matrix4 scaleMatrix = Matrix4.CreateScale(Vector3.One * 100f);
            Matrix4 transMatrix = Matrix4.CreateRotationY(rng.NextFloat() * 360);
            transMatrix *= Matrix4.CreateTranslation(position + Vector3.UnitY * 7f);
            model.Transform(scaleMatrix * transMatrix);

            model.Color(AssetManager.ColorCode0, region.Colors.WoodColor);
            model.Color(AssetManager.ColorCode1, region.Colors.LeavesColor);
            model.Color(AssetManager.ColorCode2, region.Colors.LeavesColor  * .8f);

            model.Extradata.AddRange(model.GenerateWindValues());
            float treeRng = Utils.Rng.NextFloat();
            for (var i = 0; i < model.Extradata.Count; i++)
            {
                model.Extradata[i] = Mathf.Pack(new Vector2(model.Extradata[i] * 2.5f, treeRng), 2048);
            }
            model.GraduateColor(Vector3.UnitY);

            var shapes = CacheManager.GetShape(originalModel).DeepClone();
            for (var i = 0; i < shapes.Count; i++)
            {
                shapes[i].Transform(scaleMatrix * transMatrix);
            }

            var underWater = region.Generation.GetHeight(position.X, position.Z, null, out _) < BiomePool.SeaLevel;
            Executer.ExecuteOnMainThread(delegate
            {
                var chestOffset = Vector3.UnitZ * 10f + Vector3.UnitX * -80f;
                var chestPosition = Vector3.TransformPosition(chestOffset, transMatrix);
                IEntity treeBoss = null;
                if (!underWater)
                {
                    treeBoss = BossGenerator.Generate(
                        new [] { MobType.Beetle, MobType.Gorilla },
                        Vector3.TransformPosition(chestOffset - Vector3.UnitZ * 50, transMatrix),
                        rng);
                    ((GiantTree)Structure.WorldObject).Boss = treeBoss;
                }

                var chest = World.SpawnChest(
                    chestPosition,
                    ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon))
                    );
                chest.Condition += () => treeBoss == null || treeBoss.IsDead;
                chest.Rotation = Vector3.UnitY * 90f;
                Structure.WorldObject.AddChildren(chest);
            });
            Structure.AddCollisionShape(shapes.ToArray());
            Structure.AddStaticElement(model);
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            return base.Setup(TargetPosition, Rng, new GiantTree(TargetPosition));
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng)
        {
            var height = Biome.Generation.GetHeight( TargetPosition.X, TargetPosition.Z, null, out _);
            return Rng.Next(0, 100) == 1 && height > BiomePool.SeaLevel || Rng.Next(0, 500) == 1 && height <= BiomePool.SeaLevel;
        }
    }
}
