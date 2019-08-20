using System;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.ModuleSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using Hedra.Items;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class FishingPostDesign : SimpleStructureDesign<FishingPost>
    {
        public override int PlateauRadius => 48;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.FishingPostIcon);
        protected override int StructureChance => 4;//StructureGrid.FishingPostChance;
        protected override CacheItem? Cache => null;
        protected override FishingPost Create(Vector3 Position, float Size)
        {
            return new FishingPost(Position);
        }
        
        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            var offset = World.ToChunkSpace(TargetPosition);
            SearchForShore(offset, World.BiomePool.GetRegion(offset.ToVector3()), out var targetPosition);
            var direction = (offset - targetPosition.Xz).ToVector3().NormalizedFast();
            var structure = base.Setup(targetPosition, Rng, Create(targetPosition, EffectivePlateauRadius));
            structure.Mountain.Radius = 256;
            structure.Parameters.Set("DockDirection", direction);
            structure.Parameters.Set("DockPosition", targetPosition + structure.Mountain.Radius * .65f * direction);
            structure.AddGroundwork(new RoundedGroundwork(structure.Position, 64, PathType)
            {
                NoPlants = NoPlantsZone,
                BonusHeight = -.0f
            });
            structure.AddGroundwork(new LineGroundwork(structure.Position.Xz, structure.Parameters.Get<Vector3>("DockPosition").Xz + direction.Xz * Chunk.BlockSize)
            {
                Width = 16,
                Type = BlockType.StonePath,
                BonusHeight = -0.05f
            });
            return structure;
        }

        protected override void DoBuild(CollidableStructure Structure, Matrix4 Rotation, Matrix4 Translation, Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            AddDockModel(Structure);
            AddModel(Structure, "Assets/Env/Structures/FishingSettlement/Fountain0.ply", Vector3.One * 12f, Structure.Position + Vector3.UnitY * Structure.Groundworks[0].BonusHeight);
            var decorations = new string[]
            {
                "Assets/Env/Structures/FishingSettlement/Crates0.ply",
                "Assets/Env/Structures/FishingSettlement/PunchingBag0.ply",
                "Assets/Env/Structures/FishingSettlement/Caravan0.ply",
            };
            var initialOffset = Rng.Next(0, decorations.Length);
            var count = Rng.Next(decorations.Length, decorations.Length * 2);
            var angle = 0f;
            var circle = (float) (2f * Math.PI);
            for (var i = initialOffset; i < count; i++)
            {
                var k = Mathf.Modulo(i, decorations.Length);
                var distance = 100 + 28 * Rng.NextFloat();
                var dir = new Vector3((float) Math.Cos(angle), 0, (float) Math.Sin(angle));
                if(Vector3.Dot(dir, Structure.Parameters.Get<Vector3>("DockDirection")) < 0.85f)
                    AddModel(Structure, decorations[k], Vector3.One * 12f, Structure.Position + dir * distance);
                angle += circle / count;
            }
        }

        private void AddFisherman(CollidableStructure Structure)
        {
            var fisherman  = World.WorldBuilding.SpawnHumanoid(
                HumanType.Farmer,
                Structure.Position
            );
            fisherman.SetWeapon(ItemPool.Grab(ItemType.FishingRod).Weapon);
            fisherman.LeftWeapon.Attack1(fisherman);
            ((FishingPost) Structure.WorldObject).Fisherman = fisherman;
        }

        private static void AddModel(CollidableStructure Structure, string Path, Vector3 Scale, Vector3 Position)
        {
            var model = DynamicCache.Get(Path, Scale);
            model.Transform(Matrix4.CreateTranslation(Position));
            Structure.AddStaticElement(model);
        }

        private void AddDockModel(CollidableStructure Structure)
        {
            var fishingPost = CacheManager.GetModel(CacheItem.FishingPost);
            var model = fishingPost.ShallowClone();
            var euler = Physics.DirectionToEuler(Structure.Parameters.Get<Vector3>("DockDirection"));
            var transformationMatrix = Matrix4.CreateRotationY(euler.Y * Mathf.Radian);
            transformationMatrix *= Matrix4.CreateTranslation(Structure.Parameters.Get<Vector3>("DockPosition").Xz.ToVector3() + Structure.Position.Y * Vector3.UnitY);
            model.Transform(transformationMatrix);

            var shapes = CacheManager.GetShape(fishingPost).DeepClone();
            for (var i = 0; i < shapes.Count; i++)
            {
                shapes[i].Transform(transformationMatrix);
            }

            Structure.AddStaticElement(model);
            Structure.AddCollisionShape(shapes.ToArray());
        }

        protected override bool ShouldBuild(Vector3 NewPosition, CollidableStructure[] Items, StructureDesign[] Designs)
        {
            var offset = World.ToChunkSpace(NewPosition);
            SearchForShore(offset, World.BiomePool.GetRegion(offset.ToVector3()), out var targetPosition);
            return base.ShouldBuild(targetPosition, Items, Designs);
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng)
        {
            return Rng.Next(1, 2) == 1 && IsWater(ChunkOffset.ToVector3(), Biome) && SearchForShore(ChunkOffset, Biome, out _);
        }

        private static bool SearchForShore(Vector2 Offset, Region Biome, out Vector3 Position)
        {
            var directions = new []
            {
                Vector2.UnitX.ToVector3(),
                Vector2.UnitY.ToVector3(),
                Vector2.One.ToVector3(),
            };
            for (var i = 0; i < directions.Length; ++i)
            {
                var st = IsWater(Offset.ToVector3(), Biome);
                var edge = Offset.ToVector3() + directions[i] * Chunk.Width;
                if (IsWater(edge, Biome)) continue;
                for (var j = 0; j < Chunk.Width; j += (int) Chunk.BlockSize)
                {
                    if (IsWater(Offset.ToVector3() + j * directions[i], Biome)) continue;
                    Position = Offset.ToVector3() + directions[i] * j;
                    return true;
                }
            }
            Position = Vector3.Zero;
            return false;
        }
        
        private static bool IsWater(Vector3 TargetPosition, Region Biome)
        {
            return (Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out _) < BiomePool.SeaLevel-1/* || Math.Abs(LandscapeGenerator.River(TargetPosition.Xz)) > 0.005f*/);
        }
    }
}