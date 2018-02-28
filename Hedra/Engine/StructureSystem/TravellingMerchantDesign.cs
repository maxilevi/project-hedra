using System;
using System.Collections.Generic;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class TravellingMerchantDesign : StructureDesign
    {
        public override int Radius { get; set; } = 512;

        public override void Build(Vector3 Position, CollidableStructure Structure) {

            var model = AssetManager.PlyLoader("Assets/Env/MerchantCart.ply", Vector3.One * 4.5f);
            var underChunk = World.GetChunkAt(Position);

            Matrix4 transMatrix = Matrix4.CreateScale(1);
            transMatrix *= Matrix4.CreateTranslation(Position);
            model.Transform(transMatrix);

            var merchant = new TravellingMerchant(Position);
            List<CollisionShape> shapes = AssetManager.LoadCollisionShapes("Assets/Env/MerchantCart.ply", 14, Vector3.One * 4.5f);
            for (int i = 0; i < shapes.Count; i++)
            {
                shapes[i].Transform(transMatrix);
                underChunk.AddCollisionShape(shapes[i]);
            }
            merchant.Position = Position + Vector3.UnitX * -12f;

            underChunk.AddStaticElement(model);
            underChunk.Blocked = true;

            World.AddStructure(merchant);
            ThreadManager.ExecuteOnMainThread(() => World.QuestManager.SpawnHumanoid(HumanType.TravellingMerchant, Position));
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Vector2 NewOffset, Random Rng)
        {
            World.StructureGenerator.MerchantSpawned = true;

            BlockType type;
            float height = BiomeGenerator.GetHeight(TargetPosition.X, TargetPosition.Z, null, out type);

            World.StructureGenerator.MerchantPosition = TargetPosition;

            var plateau = new Plateau(TargetPosition, 48, 4, height);
            World.QuestManager.AddPlateau(plateau);
            return new CollidableStructure(this, TargetPosition, plateau);
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Random Rng)
        {
            BlockType type;
            float height = BiomeGenerator.GetHeight(TargetPosition.X, TargetPosition.Z, null, out type);

            return !(Math.Abs(ChunkOffset.X - 50000) > 10000 || Math.Abs(ChunkOffset.Y - 50000) > 10000) &&
                   Rng.Next(0, 100) == 1 && BiomeGenerator.PathFormula(TargetPosition.X, TargetPosition.Y) > 0 && height > 0;
        }

        public override bool MeetsRequirements(Vector2 ChunkOffset)
        {
            return base.MeetsRequirements(ChunkOffset) && !World.StructureGenerator.MerchantSpawned;
        }
    }
}
