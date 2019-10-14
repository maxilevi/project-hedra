using System;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.EntitySystem.BossSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Localization;
using Hedra.Rendering;
using System.Numerics;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class GiantTreeDesign : CompletableStructureDesign<GiantTree>
    {
        public override int PlateauRadius { get; } = 700;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.BossIcon);

        public override void Build(CollidableStructure Structure)
        {
            var position = Structure.Position;
            var region = World.BiomePool.GetRegion(position);
            var rng = new Random((int)(position.X / 11 * (position.Z / 13)));
            var originalModel = CacheManager.GetModel(CacheItem.GiantTree);
            var model = originalModel.ShallowClone();

            var scaleMatrix = Matrix4x4.CreateScale(Vector3.One * 100f);
            var transMatrix = Matrix4x4.CreateRotationY(rng.NextFloat() * 360 * Mathf.Radian);
            transMatrix *= Matrix4x4.CreateTranslation(position + Vector3.UnitY * 7f);
            model.Transform(scaleMatrix * transMatrix);

            model.Color(AssetManager.ColorCode0, region.Colors.WoodColor);
            model.Color(AssetManager.ColorCode1, region.Colors.LeavesColor);
            model.Color(AssetManager.ColorCode2, region.Colors.LeavesColor  * .8f);

            model.AddWindValues();
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
            Structure.AddCollisionShape(shapes.ToArray());
            Structure.AddStaticElement(model);
            DoWhenChunkReady(position, P => PlaceBoss(P, region, Structure, transMatrix, rng), Structure);
        }

        private void PlaceBoss(Vector3 Position, Region Region, CollidableStructure Structure, Matrix4x4 TransMatrix, Random Rng)
        {
            var underWater = Region.Generation.GetMaxHeight(Position.X, Position.Z) < BiomePool.SeaLevel;
            var chestOffset = Vector3.UnitZ * 10f + Vector3.UnitX * -80f;
            var chestPosition = Vector3.Transform(chestOffset, TransMatrix);
            IEntity treeBoss = null;
            if (!underWater)
            {
                treeBoss = BossGenerator.Generate(
                    new [] { MobType.GiantBeetle, MobType.GorillaWarrior, MobType.Troll },
                    Vector3.Transform(chestOffset - Vector3.UnitZ * 50, TransMatrix),
                    Rng);
                ((GiantTree)Structure.WorldObject).Boss = treeBoss;
            }

            var chest = World.SpawnChest(
                chestPosition,
                ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon))
            );
            chest.Condition += () => treeBoss == null || treeBoss.IsDead;
            chest.Rotation = Vector3.UnitY * 90f;
            ((GiantTree) Structure.WorldObject).Chest = chest;
            Structure.WorldObject.AddChildren(chest);
        }
        
        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            return base.Setup(TargetPosition, Rng, new GiantTree(TargetPosition));
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng)
        {
            var height = Biome.Generation.GetMaxHeight( TargetPosition.X, TargetPosition.Z);
            return Rng.Next(0, StructureGrid.GiantTreeChance) == 1 && height > BiomePool.SeaLevel 
                   || Rng.Next(0, StructureGrid.WaterGiantTreeChance) == 1 && height <= BiomePool.SeaLevel;
        }
        
        public override string DisplayName => Translations.Get("structure_giant_tree");

        protected override string GetShortDescription(GiantTree Structure)
        {
            return Translations.Get("quest_complete_structure_short_giant_tree", Structure.Boss.Name.ToUpperInvariant());
        }

        protected override string GetDescription(GiantTree Structure)
        {
            return Translations.Get("quest_complete_structure_description_giant_tree", Structure.Boss.Name.ToUpperInvariant(), DisplayName);
        }
    }
}
