using System;
using System.Collections.Generic;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    internal class TravellingMerchantDesign : StructureDesign
    {
        public override int Radius { get; set; } = 512;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.MerchantIcon);

        public override void Build(Vector3 Position, CollidableStructure Structure)
        {
            var originalModel = CacheManager.GetModel(CacheItem.MerchantCart);
            var model = originalModel.ShallowClone();
            var underChunk = World.GetChunkAt(Position);

            Matrix4 transMatrix = Matrix4.CreateScale(4.5f);
            transMatrix *= Matrix4.CreateTranslation(Position);
            model.Transform(transMatrix);

            var merchant = new TravellingMerchant(Position);
            var shapes = CacheManager.GetShape(originalModel).DeepClone();
            for (int i = 0; i < shapes.Count; i++)
            {
                shapes[i].Transform(transMatrix);
                underChunk.AddCollisionShape(shapes[i]);
            }
            merchant.Position = Position + Vector3.UnitX * -12f;

            underChunk.AddStaticElement(model);
            underChunk.Blocked = true;

            World.AddStructure(merchant);
            Executer.ExecuteOnMainThread(() => World.WorldBuilding.SpawnHumanoid(HumanType.TravellingMerchant, Position));
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Vector2 NewOffset, Region Biome, Random Rng)
        {
            World.StructureGenerator.MerchantSpawned = true;
            World.StructureGenerator.MerchantPosition = TargetPosition;
            var plateau = new Plateau(TargetPosition, 48);
            World.WorldBuilding.AddPlateau(plateau);
            return new CollidableStructure(this, TargetPosition, plateau);
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, Random Rng)
        {
            BlockType type;
            float height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out type);

            return !(Math.Abs(ChunkOffset.X - GameSettings.SpawnPoint.X) > 10000 || Math.Abs(ChunkOffset.Y - GameSettings.SpawnPoint.Y) > 10000) &&
                   Rng.Next(0, 40) == 1 && BiomeGenerator.PathFormula(TargetPosition.X, TargetPosition.Y) > 0 && height > 0 && !World.StructureGenerator.MerchantSpawned;
        }
    }
}
