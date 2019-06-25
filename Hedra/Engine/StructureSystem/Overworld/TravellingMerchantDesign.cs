using System;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Generation;
using Hedra.Engine.Localization;
using Hedra.Engine.WorldBuilding;
using Hedra.Localization;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class TravellingMerchantDesign : FindableStructureDesign
    {
        public static bool Spawned { get; private set; }
        public override int PlateauRadius { get; } = 80;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.MerchantIcon);
        public override VertexData QuestIcon => Icon;

        public override void Build(CollidableStructure Structure)
        {
            var originalModel = CacheManager.GetModel(CacheItem.MerchantCart);
            var model = originalModel.ShallowClone();

            var transMatrix = Matrix4.CreateScale(4.5f);
            transMatrix *= Matrix4.CreateTranslation(Structure.Position);
            model.Transform(transMatrix);

            var shapes = CacheManager.GetShape(originalModel).DeepClone();
            for (var i = 0; i < shapes.Count; i++)
            {
                shapes[i].Transform(transMatrix);
            }
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

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng)
        {
            var height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out _);

            return Math.Abs(ChunkOffset.X - World.SpawnPoint.X) < 10000 && Math.Abs(ChunkOffset.Y - World.SpawnPoint.Y) < 10000 &&
                   Rng.Next(0, StructureGrid.TravellingMerchantChance) == 1 && BiomeGenerator.PathFormula(TargetPosition.X, TargetPosition.Y) > 0 && height > BiomePool.SeaLevel && !Spawned
                   && Math.Abs(LandscapeGenerator.River(TargetPosition.Xz)) < 0.005f;
        }
        
        public override string DisplayName => Translations.Get("structure_travelling_merchant");
    }
}
