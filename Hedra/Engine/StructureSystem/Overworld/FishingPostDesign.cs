using System;
using System.Linq;
using Hedra.AISystem.Humanoid;
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
using Hedra.Engine.Scenes;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.WorldBuilding;
using Hedra.Items;
using Hedra.Rendering;
using Microsoft.Scripting.Utils;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class FishingPostDesign : SimpleStructureDesign<FishingPost>
    {
        public override int PlateauRadius => 48;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.FishingPostIcon);
        protected override int StructureChance => 4;//StructureGrid.FishingPostChance;
        protected override CacheItem? Cache => null;
        private const int RealPlateauRadius = 256 + 64;
        protected override BlockType PathType => BlockType.Path;

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
            structure.Mountain.Radius = RealPlateauRadius;
            structure.Parameters.Set("DockDirection", direction);
            structure.Parameters.Set("DockPosition", targetPosition + structure.Mountain.Radius * .65f * direction);
            structure.AddGroundwork(new RoundedGroundwork(structure.Position, 64, PathType)
            {
                NoPlants = NoPlantsZone,
                BonusHeight = -.0f
            });
            structure.AddGroundwork(new LineGroundwork(structure.Position.Xz, structure.Parameters.Get<Vector3>("DockPosition").Xz + direction.Xz * Chunk.BlockSize * 4f)
            {
                Width = 16,
                Type = PathType,
                BonusHeight = -0.05f
            });
            AddDockModel(structure, Rng);
            AddSmallDock(structure, Rng);
            AddBoatDecorations(structure, Rng);
            AddFountain(structure, Rng);
            AddDecorations(structure, Rng);
            //AddCampfires(structure, Rng);
            structure.AddGroundwork(new RoundedGroundwork(structure.Position, 32, BlockType.StonePath));
            return structure;
        }

        private void AddFishingShack()
        {
            
        }
        
        private void AddSmallDock(CollidableStructure Structure, Random Rng)
        {
            var position = FindSmallDockPosition(Structure, Rng, out var isValid).ToVector3();
            if (isValid)
            {
                var toDock = (position - Structure.Position).NormalizedFast();
                var euler = Physics.DirectionToEuler(toDock);
                var transformation = Matrix4.CreateRotationY(euler.Y * Mathf.Radian) *
                                     Matrix4.CreateTranslation(position + Structure.Position.Y * Vector3.UnitY);
                AddModel(Structure, "Assets/Env/Structures/FishingSettlement/SmallDock0.ply", FishingPostScale, transformation);
                Structure.AddGroundwork(new LineGroundwork(Structure.Position.Xz, transformation.ExtractTranslation().Xz)
                {
                    Width = 12,
                    Type = BlockType.Path,
                    BonusHeight = 0
                });
            }
        }

        private static Vector2 FindSmallDockPosition(CollidableStructure Structure, Random Rng, out bool IsValid)
        {
            IsValid = true;
            var mainDockPosition = Structure.Parameters.Get<Vector3>("DockPosition").Xz;
            var mainDockDirection = Structure.Parameters.Get<Vector3>("DockDirection").Xz;
            var positions = new Vector2[]
            {
                mainDockPosition + mainDockDirection.PerpendicularLeft * 32f,
                mainDockPosition + mainDockDirection.PerpendicularRight * 32f
            }.Where(P => IsWater(P.ToVector3(), World.BiomePool.GetRegion(P.ToVector3()))).ToArray();
            if (positions.Length == 0)
                IsValid = false;
            return IsValid ? positions[Rng.Next(0, positions.Length)] : default(Vector2);

        }

        private void AddBoatDecorations(CollidableStructure Structure, Random Rng)
        {
            var count = 0;
            var targetCount = Rng.Next(5, 11);
            var iteration = 0;
            const int maxIterations = 50;
            while(iteration < maxIterations && count < targetCount)
            {
                var point = Structure.Parameters.Get<Vector3>("DockPosition").Xz +
                            Structure.Parameters.Get<Vector3>("DockDirection").Xz * FishingPostScale.Xz * 16 +
                            new Vector2(Utils.Rng.NextFloat() * 256 - 128, Utils.Rng.NextFloat() * 256 - 128);
                if (IsWater(point.ToVector3(), World.BiomePool.GetRegion(point.ToVector3())) && !Structure.Mountain.Collides(point))
                {
                    SpawnFisherman(Structure, point.ToVector3());
                    count++;
                }

                iteration++;
            }
        }

        private void SpawnFisherman(CollidableStructure Structure, Vector3 Position)
        {
            var fisherman = World.WorldBuilding.SpawnHumanoid(HumanType.Fisherman, Position);
            fisherman.Rotation = new Vector3(0, Utils.Rng.NextFloat() * 360, 0);
            fisherman.AddComponent(new FishermanAIComponent(fisherman, Position.Xz, Vector2.One * 128f));
            ((FishingPost)Structure.WorldObject).Fishermans.Add(fisherman);
        }

        private void AddCampfires(CollidableStructure Structure, Random Rng)
        {
            const int maxIterations = 64;
            var pointCount = Rng.Next(3, 5);
            var count = 0;
            var iterations = 0;
            while(count < pointCount && iterations < maxIterations)
            {
                iterations++;
                var point = Structure.Position + new Vector3(Rng.NextFloat() * RealPlateauRadius - RealPlateauRadius * .5f, 0, Rng.NextFloat() * RealPlateauRadius - RealPlateauRadius  * .5f);
                if(Structure.Groundworks.Any(G => G.Affects(point.Xz))) continue;
                var euler = Physics.DirectionToEuler((Structure.Position - point).NormalizedFast()) + Vector3.UnitY * 90;
                CampfireDesign.BuildBaseCampfire(point, euler, Structure, Rng, out var transformationMatrix);
                if(Rng.Next(0, 3) == 1)
                    CampfireDesign.SpawnMat(point, euler, transformationMatrix, Structure);
                Structure.WorldObject.AddChildren(new Campfire(point));
                Structure.AddGroundwork(new RoundedGroundwork(transformationMatrix.ExtractTranslation(), 24, BlockType.Path));
                count++;
            }
        }

        private static void AddFountain(CollidableStructure Structure, Random Rng)
        {
            const string path = "Assets/Env/Structures/FishingSettlement/Fountain0.ply";
            var fountainTransformation =
                Matrix4.CreateTranslation(Structure.Position + Vector3.UnitY * Structure.Groundworks[0].BonusHeight);
            AddModel(Structure, path, FishingPostScale - Vector3.One, fountainTransformation, new SceneSettings
            {
                LightRadius = PointLight.DefaultRadius * 2f
            });

            var output = MarketBuilder.DoBuildMarket(Structure.Position, Rng, 4f, 6);
            Structure.AddStaticElement(output.Models.ToArray());
            Structure.AddCollisionShape(output.Shapes.ToArray());
        }
        
        private static void AddDecorations(CollidableStructure Structure, Random Rng)
        {
            var decorations = new string[]
            {
                "Assets/Env/Structures/FishingSettlement/Crates0.ply",
                "Assets/Env/Structures/FishingSettlement/PunchingBag0.ply",
                "Assets/Env/Structures/FishingSettlement/Caravan0.ply",
            };
            var initialOffset = Rng.Next(0, decorations.Length);
            var count = Rng.Next(4, 9);
            var angle = 0f;
            var circle = (float) (2f * Math.PI);
            for (var i = initialOffset; i < count + initialOffset; i++)
            {
                var k = Mathf.Modulo(i, decorations.Length);
                var distance = 96f + 80 * Rng.NextFloat();
                var dir = new Vector3((float) Math.Cos(angle), 0, (float) Math.Sin(angle));
                if (!IsInDockPath(dir, Structure))
                {
                    var transformation = Matrix4.CreateRotationY((Physics.DirectionToEuler(dir).Y - 90) * Mathf.Radian) * Matrix4.CreateTranslation(Structure.Position + dir * distance);
                    var scale = FishingPostScale - Vector3.One;
                    AddModel(Structure, decorations[k], scale, transformation);
                    Structure.AddGroundwork(new LineGroundwork(Structure.Position.Xz, transformation.ExtractTranslation().Xz)
                    {
                        Width = 10,
                        Type = BlockType.Path,
                        BonusHeight = 0
                    });
                    Structure.AddGroundwork(new RoundedGroundwork(transformation.ExtractTranslation(), 24, BlockType.Path));
                }

                angle += circle / count;
            }
        }

        private static bool IsInDockPath(Vector3 DirectionToCenter, CollidableStructure Structure)
        {
            return Vector3.Dot(DirectionToCenter, Structure.Parameters.Get<Vector3>("DockDirection")) > 0.975f;
        }

        private void AddFisherman(CollidableStructure Structure)
        {
            var fisherman  = World.WorldBuilding.SpawnHumanoid(
                HumanType.Farmer,
                Structure.Position
            );
            fisherman.SetWeapon(ItemPool.Grab(ItemType.FishingRod).Weapon);
            fisherman.LeftWeapon.Attack1(fisherman);
            ((FishingPost) Structure.WorldObject).Fishermans.Add(fisherman);
        }

        private static void AddModel(CollidableStructure Structure, string Path, Vector3 Scale, Matrix4 Transformation, SceneSettings Settings = null)
        {
            var model = DynamicCache.Get(Path, Scale);
            var shapes = DynamicCache.GetShapes(Path, Scale);
            Structure.AddStaticElement(model.Transform(Transformation));
            Structure.AddCollisionShape(shapes.Select(S => S.Transform(Transformation)).ToArray());
            SceneLoader.LoadIfExists(Structure, Path, Scale, Transformation, Settings ?? new SceneSettings());
        }

        private static void AddDockModel(CollidableStructure Structure, Random Rng)
        {
            const int maxDocks = 3;
            var dockNumber = Rng.Next(0, maxDocks);
            var basePath = $"Assets/Env/Structures/FishingSettlement/FishingDock{dockNumber}";
            var model = DynamicCache.Get($"{basePath}-Mesh.ply", FishingPostScale);
            var euler = Physics.DirectionToEuler(Structure.Parameters.Get<Vector3>("DockDirection"));
            var transformationMatrix = Matrix4.CreateRotationY(euler.Y * Mathf.Radian);
            transformationMatrix *= Matrix4.CreateTranslation(Structure.Parameters.Get<Vector3>("DockPosition").Xz.ToVector3() + Structure.Position.Y * Vector3.UnitY);
            model.Transform(transformationMatrix);

            var shapes = DynamicCache.GetShapes($"{basePath}-Colliders.ply", FishingPostScale).DeepClone();
            for (var i = 0; i < shapes.Count; i++)
            {
                shapes[i].Transform(transformationMatrix);
            }

            var settings = new SceneSettings
            {
                LightRadius = PointLight.DefaultRadius * 2
            };
            var sceneModel = DynamicCache.Get($"{basePath}-Scene.ply", FishingPostScale);
            sceneModel.Transform(transformationMatrix);
            SceneLoader.Load(Structure, sceneModel, settings);
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
        
        private static Vector3 FishingPostScale => Vector3.One * 11f;
    }
}