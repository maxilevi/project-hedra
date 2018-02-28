using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class WoodenFortDesign : StructureDesign
    {
        public override int Radius { get; set; } = 512;

        public override void Build(Vector3 Position, CollidableStructure Structure)
        {
            var chunk = World.GetChunkAt(Position);
            var rng = new Random(World.Seed + 64432 + chunk.OffsetX + chunk.OffsetZ + (int)Position.X + (int)Position.Y);
            var fortModel = AssetManager.PlyLoader("Assets/Env/Fort1.ply", Vector3.One * 1.5f, Vector3.Zero, Vector3.Zero);

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
            
            /*ThreadManager.ExecuteOnMainThread(delegate
            {

                Chest Prize = new Chest(Vector3.TransformPosition(Vector3.UnitX * -200f, transMatrix), new Item.InventoryItem(Item.ItemType.Random));
                Prize.Condition += () => (TreeBoss == null || TreeBoss.IsDead);

                TreeBoss.Position = Prize.Position.Xz.ToVector3();
                TreeBoss.SearchComponent<BossAIComponent>().Protect = () => Prize.Position;

                World.AddStructure(Prize);
            });*/

        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Vector2 NewOffset, Random Rng)
        {
            return new CollidableStructure(this, NewOffset.ToVector3(), null);
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Random Rng)
        {
            return false;//350
        }
    }
}
