using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.WorldBuilding;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class TravellingMerchantDesign : CompletableStructureDesign<TravellingMerchant>
    {
        public static bool Spawned { get; private set; }
        public override int PlateauRadius { get; } = 80;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.MerchantIcon);
        public override bool CanSpawnInside => true;

        public override string DisplayName => Translations.Get("structure_travelling_merchant");

        public override void Build(CollidableStructure Structure)
        {
            var originalModel = CacheManager.GetModel(CacheItem.MerchantCart);
            var model = originalModel.ShallowClone();

            var transMatrix = Matrix4x4.CreateScale(4.5f);
            transMatrix *= Matrix4x4.CreateTranslation(Structure.Position);
            model.Transform(transMatrix);

            var shapes = CacheManager.GetShape(originalModel).DeepClone();
            for (var i = 0; i < shapes.Count; i++) shapes[i].Transform(transMatrix);
            Structure.AddStaticElement(model);
            Structure.AddCollisionShape(shapes.ToArray());
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            Spawned = true;
            var structure = base.Setup(TargetPosition, Rng, new TravellingMerchant(TargetPosition));
            structure.Mountain.Radius = 48;
            return structure;
        }

        protected override bool SetupRequirements(ref Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome,
            IRandom Rng)
        {
            if (Rng.Next(0, StructureGrid.TravellingMerchantChance) != 1) return false;
            var height = Biome.Generation.GetMaxHeight(TargetPosition.X, TargetPosition.Z);

            return Math.Abs(ChunkOffset.X - World.SpawnPoint.X) < 10000 &&
                   Math.Abs(ChunkOffset.Y - World.SpawnPoint.Y) < 10000 &&
                   BiomeGenerator.PathFormula(TargetPosition.X, TargetPosition.Y) > 0 && height > BiomePool.SeaLevel &&
                   !Spawned
                   && Math.Abs(Biome.Generation.RiverAtPoint(TargetPosition.X, TargetPosition.Z)) < 0.005f;
        }

        protected override string GetShortDescription(TravellingMerchant Structure)
        {
            return Translations.Get("quest_complete_structure_short_trade_travelling_merchant",
                Structure.Merchant.Name);
        }

        protected override string GetDescription(TravellingMerchant Structure)
        {
            return Translations.Get("quest_complete_structure_description_trade_travelling_merchant",
                Structure.Merchant.Name, Structure.ItemsToBuy.ToString());
        }
    }
}