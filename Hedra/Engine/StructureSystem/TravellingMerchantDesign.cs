using System;
using System.Collections.Generic;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ComplexMath;
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
            var position = Structure.Position;
            var originalModel = CacheManager.GetModel(CacheItem.MerchantCart);
            var model = originalModel.ShallowClone();

            Matrix4 transMatrix = Matrix4.CreateScale(4.5f);
            transMatrix *= Matrix4.CreateTranslation(position);
            model.Transform(transMatrix);

            var merchant = new TravellingMerchant(position);
            var shapes = CacheManager.GetShape(originalModel).DeepClone();
            for (int i = 0; i < shapes.Count; i++)
            {
                shapes[i].Transform(transMatrix);
            }
            Structure.AddStaticElement(model);
            Structure.AddCollisionShape(shapes.ToArray());

            World.AddStructure(merchant);
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            World.StructureGenerator.MerchantSpawned = true;
            World.StructureGenerator.MerchantPosition = TargetPosition;
            var structure = base.Setup(TargetPosition, Rng);
            structure.Mountain.Radius = 48;
            return structure;
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng)
        {
            float height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out _);

            return !(Math.Abs(ChunkOffset.X - GameSettings.SpawnPoint.X) > 10000 || Math.Abs(ChunkOffset.Y - GameSettings.SpawnPoint.Y) > 10000) &&
                   Rng.Next(0, 40) == 1 && BiomeGenerator.PathFormula(TargetPosition.X, TargetPosition.Y) > 0 && height > BiomePool.SeaLevel && !World.StructureGenerator.MerchantSpawned;
        }
    }
}
