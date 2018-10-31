using System;
using System.Collections.Generic;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class TravellingMerchantDesign : StructureDesign
    {
        public override int Radius { get; set; } = 512;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.MerchantIcon);

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
            World.StructureHandler.MerchantSpawned = true;
            World.StructureHandler.MerchantPosition = TargetPosition;
            var structure = base.Setup(TargetPosition, Rng, new TravellingMerchant(TargetPosition - Vector3.UnitX * 12f));
            structure.Mountain.Radius = 48;
            return structure;
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng)
        {
            var height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out _);

            return !(Math.Abs(ChunkOffset.X - GameSettings.SpawnPoint.X) > 10000 || Math.Abs(ChunkOffset.Y - GameSettings.SpawnPoint.Y) > 10000) &&
                   Rng.Next(0, 40) == 1 && BiomeGenerator.PathFormula(TargetPosition.X, TargetPosition.Y) > 0 && height > BiomePool.SeaLevel && !World.StructureHandler.MerchantSpawned;
        }
    }
}
