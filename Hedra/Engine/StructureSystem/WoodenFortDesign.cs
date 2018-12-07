using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class WoodenFortDesign : StructureDesign
    {
        public override int Radius { get; set; } = 256;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.BossIcon);

        public override void Build(CollidableStructure Structure)
        {
            var position = Structure.Position;
            var rng = new Random(World.Seed + 64432 + (int)position.X + (int)position.Z);
            var fortModel = AssetManager.PLYLoader("Assets/Env/Fort1.ply", Vector3.One * 1.5f, Vector3.Zero, Vector3.Zero);

            var transMatrix = Matrix4.Identity;
            transMatrix *= Matrix4.CreateRotationY(rng.NextFloat() * 360 * Mathf.Radian);
            transMatrix *= Matrix4.CreateTranslation(position);
            fortModel.Transform(transMatrix);

            var shapes = AssetManager.LoadCollisionShapes("Assets/Env/Fort1.ply", 91, Vector3.One * 1.5f);
            for (var i = 0; i < shapes.Count; i++)
            {
                shapes[i].Transform(transMatrix);
            }
            Structure.AddStaticElement(fortModel);
            Structure.AddCollisionShape(shapes.ToArray());
            
            Executer.ExecuteOnMainThread(delegate
            {

                //var Prize = new Chest(Position, new InventoryItem(ItemType.Random));
                //Prize.Condition += () => (TreeBoss == null || TreeBoss.IsDead);

                //World.AddStructure(Prize);
            });
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            return base.Setup(TargetPosition, Rng, null);
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng)
        {
            var height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out _);
            return false && Rng.Next(0, 5) == 1 && height > 0;
        }
    }
}
