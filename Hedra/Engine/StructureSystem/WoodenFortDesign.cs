using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    internal class WoodenFortDesign : StructureDesign
    {
        public override int Radius { get; set; } = 256;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.BossIcon);

        public override void Build(Vector3 Position, CollidableStructure Structure)
        {
            var chunk = World.GetChunkAt(Position);
            var rng = new Random(World.Seed + 64432 + chunk.OffsetX + chunk.OffsetZ + (int)Position.X + (int)Position.Y);
            var fortModel = AssetManager.PLYLoader("Assets/Env/Fort1.ply", Vector3.One * 1.5f, Vector3.Zero, Vector3.Zero);

            var transMatrix = Matrix4.Identity;
            transMatrix *= Matrix4.CreateRotationY(rng.NextFloat() * 360);
            transMatrix *= Matrix4.CreateTranslation(Position);
            fortModel.Transform(transMatrix);

            List<CollisionShape> shapes = AssetManager.LoadCollisionShapes("Assets/Env/Fort1.ply", 91, Vector3.One * 1.5f);
            for (int i = 0; i < shapes.Count; i++)
            {
                shapes[i].Transform(transMatrix);
                Structure.AddCollisionShape(shapes[i]);
            }
            chunk.AddStaticElement(fortModel);
            
            Executer.ExecuteOnMainThread(delegate
            {

                //var Prize = new Chest(Position, new InventoryItem(ItemType.Random));
                //Prize.Condition += () => (TreeBoss == null || TreeBoss.IsDead);

                //World.AddStructure(Prize);
            });

        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Vector2 NewOffset, Region Biome, Random Rng)
        {
            var plateau = new Plateau(TargetPosition, this.Radius);
            World.WorldBuilding.AddPlateau(plateau);
            return new CollidableStructure(this, TargetPosition, plateau);
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, Random Rng)
        {
            BlockType type;
            float height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out type);
            return false && Rng.Next(0, 50) == 1 && height > 0;
        }
    }
}
