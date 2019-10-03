using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.AISystem.Humanoid;
using Hedra.BiomeSystem;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Scenes;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.WorldBuilding;
using Hedra.Items;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class FishingPostDesign : SimpleStructureDesign<FishingPost>
    {
        public override int PlateauRadius => 192;
        protected override float EffectivePlateauRadius => 256;
        public override VertexData Icon => CacheManager.GetModel(CacheItem.FishingPostIcon);
        protected override int StructureChance => 4;//StructureGrid.FishingPostChance;
        protected override CacheItem? Cache => null;
        protected override BlockType PathType => BlockType.Path;
        private const int CenterRadius = 64;

        protected override FishingPost Create(Vector3 Position, float Size)
        {
            return new FishingPost(Position);
        }
        
        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            var offset = World.ToChunkSpace(TargetPosition);
            SearchForShore(offset, World.BiomePool.GetRegion(offset.ToVector3()), out var targetPosition);
            var structure = base.Setup(targetPosition, Rng, Create(targetPosition, EffectivePlateauRadius));
            structure.Mountain.Radius = EffectivePlateauRadius;
            structure.AddGroundwork(new RoundedGroundwork(structure.Position, CenterRadius, PathType)
            {
                NoPlants = NoPlantsZone,
                BonusHeight = -.0f
            });
            var graph = new VillageGraph();
            AddDockModel(structure, Rng, offset, targetPosition, out var mainDockPosition, out var mainDockDirection);
            AddSmallDock(structure, Rng, mainDockPosition, mainDockDirection, out var smallDockPosition, out var smallDockDirection);
            AddBoatDecorations(structure, Rng, mainDockPosition, mainDockDirection);
            AddFountain(structure, Rng, graph);
            var decorationPoints = AddDecorations(structure, Rng, graph, mainDockDirection, smallDockDirection);
            AddFishingStand(structure, Rng, mainDockPosition, mainDockDirection, smallDockPosition);
            BuildGraph(graph, structure, smallDockPosition, mainDockPosition, decorationPoints);
            //AddCampfires(structure, Rng);
            structure.AddGroundwork(new RoundedGroundwork(structure.Position, CenterRadius * .5f, BlockType.StonePath));
            return structure;
        }

        private static void AddFishingStand(CollidableStructure Structure, Random Rng, Vector2 MainDockPosition, Vector2 MainDockDirection, Vector2? SmallDockPosition)
        {
            var targetPosition = FindSmallDockPosition(Structure, Rng, MainDockPosition, MainDockDirection, out var isValid).ToVector3();
            if (isValid)
            {
                var toPost = (targetPosition - Structure.Position).Xz.ToVector3().NormalizedFast();
                var position = toPost * Structure.Mountain.Radius * .65f + Structure.Position;
                if(SmallDockPosition.HasValue && (position.Xz - SmallDockPosition.Value).LengthSquared < 48*48) return;
                AddModel(Structure, "Assets/Env/Structures/FishingSettlement/FishingStand0.ply", Vector3.One, Matrix4.CreateTranslation(position));
            }
        }
        
        private static void AddSmallDock(CollidableStructure Structure, Random Rng, Vector2 MainDockPosition, Vector2 MainDockDirection, out Vector2? Position, out Vector2? Direction)
        {
            Direction = null;
            Position = null;
            var targetPosition = FindSmallDockPosition(Structure, Rng, MainDockPosition, MainDockDirection, out var isValid).ToVector3();
            if (isValid)
            {
                var toDock = (targetPosition - Structure.Position).Xz.ToVector3().NormalizedFast();
                var position = toDock * Structure.Mountain.Radius * .65f + Structure.Position;
                var euler = Physics.DirectionToEuler(toDock);
                var transformation = Matrix4.CreateRotationY(euler.Y * Mathf.Radian) *
                                     Matrix4.CreateTranslation(position);
                AddModel(Structure, "Assets/Env/Structures/FishingSettlement/SmallDock0.ply", FishingPostScale, transformation);
                Structure.AddGroundwork(new LineGroundwork(Structure.Position.Xz, transformation.ExtractTranslation().Xz + toDock.Xz * Chunk.BlockSize * 8f)
                {
                    Width = 12,
                    Type = BlockType.Path,
                    BonusHeight = 0
                });
                Direction = toDock.Xz;
                Position = position.Xz;
            }
        }

        private static Vector2 FindSmallDockPosition(CollidableStructure Structure, Random Rng, Vector2 MainDockPosition, Vector2 MainDockDirection, out bool IsValid)
        {
            IsValid = true;
            var positions = new []
            {
                MainDockPosition + MainDockDirection.PerpendicularLeft * (180f + Rng.NextFloat() * 256f),
                MainDockPosition + MainDockDirection.PerpendicularRight * (180f + Rng.NextFloat() * 256f)
            }.Where(P => IsWater(P.ToVector3(), World.BiomePool.GetRegion(P.ToVector3()))).ToArray();
            if (positions.Length == 0)
                IsValid = false;
            return IsValid ? positions[Rng.Next(0, positions.Length)] : default(Vector2);
        }

        private void AddBoatDecorations(CollidableStructure Structure, Random Rng, Vector2 MainDockPosition, Vector2 MainDockDirection)
        {
            var count = 0;
            var targetCount = Rng.Next(5, 9);
            var iteration = 0;
            const int maxIterations = 50;
            while(iteration < maxIterations && count < targetCount)
            {
                var point = MainDockPosition + MainDockDirection * FishingPostScale.Xz * 16 +
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
            Structure.WorldObject.AddNPCs(fisherman);
        }

        private void AddCampfires(CollidableStructure Structure, Random Rng)
        {
            const int maxIterations = 64;
            var pointCount = Rng.Next(0, 2);
            var count = 0;
            var iterations = 0;
            while(count < pointCount && iterations < maxIterations)
            {
                iterations++;
                var point = Structure.Position + new Vector3(Rng.NextFloat() * EffectivePlateauRadius - EffectivePlateauRadius * .5f, 0, Rng.NextFloat() * EffectivePlateauRadius - EffectivePlateauRadius  * .5f);
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

        private static void AddFountain(CollidableStructure Structure, Random Rng, VillageGraph Graph)
        {
            const string path = "Assets/Env/Structures/FishingSettlement/Fountain0.ply";
            var fountainTransformation =
                Matrix4.CreateTranslation(Structure.Position + Vector3.UnitY * Structure.Groundworks[0].BonusHeight);
            AddModel(Structure, path, FishingPostScale - Vector3.One, fountainTransformation, new SceneSettings
            {
                LightRadius = PointLight.DefaultRadius * 2f
            });

            var count = Rng.Next(0, 4);
            for (var i = 0; i < count; ++i)
            {
                AddVillager(Structure, Structure.Position + new Vector3(CenterRadius * Rng.NextFloat() - CenterRadius * .5f, 0, CenterRadius * Rng.NextFloat() - CenterRadius * .5f), Rng, Graph);
            }
            
            AddMerchant(Structure, Structure.Position + Vector3.UnitX * CenterRadius * .25f, Rng);
            AddMerchant(Structure, Structure.Position - Vector3.UnitX * CenterRadius * .25f, Rng);
            
            var output = MarketBuilder.DoBuildMarket(Structure.Position, Rng, 4f, 6);
            Structure.AddStaticElement(output.Models.ToArray());
            Structure.AddCollisionShape(output.Shapes.ToArray());
        }
        
        private static Vector2[] AddDecorations(CollidableStructure Structure, Random Rng, VillageGraph Graph, params Vector2?[] InvalidDirections)
        {
            var decorations = new string[]
            {
                "Assets/Env/Structures/FishingSettlement/Crates0.ply",
                "Assets/Env/Structures/FishingSettlement/PunchingBag0.ply",
                "Assets/Env/Structures/FishingSettlement/Caravan0.ply"
            };
            var initialOffset = Rng.Next(0, decorations.Length);
            var count = Rng.Next(4, 9);
            var angle = 0f;
            var circle = (float) (2f * Math.PI);
            var points = new List<Vector2>();
            for (var i = initialOffset; i < count + initialOffset; i++)
            {
                var k = Mathf.Modulo(i, decorations.Length);
                var distance = 96f + 80 * Rng.NextFloat();
                var dir = new Vector3((float) Math.Cos(angle), 0, (float) Math.Sin(angle));
                if (!IsInvalidPath(dir, InvalidDirections))
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
                    points.Add(transformation.ExtractTranslation().Xz);
                }

                angle += circle / count;
            }
            return points.ToArray();
        }

        private static bool IsInvalidPath(Vector3 DirectionToCenter, Vector2?[] InvalidDirections)
        {
            return InvalidDirections.Any(D => D.HasValue && Vector2.Dot(DirectionToCenter.Xz, D.Value) > 0.975f);
        }

        private static void AddMerchant(CollidableStructure Structure, Vector3 Position, Random Rng)
        {
            DecorationsPlacer.PlaceWhenWorldReady(Position, P =>
            {
                var human = World.WorldBuilding.SpawnHumanoid(HumanType.Merchant, Position);
                Structure.WorldObject.AddNPCs(human);
            }, () => Structure.Disposed);
        }

        private static void AddVillager(CollidableStructure Structure, Vector3 Position, Random Rng, VillageGraph Graph)
        {
            DecorationsPlacer.PlaceWhenWorldReady(Position, P =>
            {
                Builder.SpawnVillager(Position, Rng, Graph, out var humanoid);
                Structure.WorldObject.AddNPCs(humanoid);
            }, () => Structure.Disposed);
        }

        private static void BuildGraph(VillageGraph Graph, CollidableStructure Structure, Vector2? MiniDockPosition, Vector2 DockPosition, Vector2[] Decorations)
        {
            var center = Structure.Position.Xz;
            Graph.AddSymmetricEdge(center, DockPosition);
            if (MiniDockPosition.HasValue)
            {
                Graph.AddSymmetricEdge(center, MiniDockPosition.Value);
                Graph.AddSymmetricEdge(DockPosition, MiniDockPosition.Value);
            }

            for (var i = 0; i < Decorations.Length; ++i)
            {
                Graph.AddSymmetricEdge(center, Decorations[i]);
            }
        }

        private static void AddModel(CollidableStructure Structure, string Path, Vector3 Scale, Matrix4 Transformation, SceneSettings Settings = null)
        {
            var model = DynamicCache.Get(Path, Scale);
            var shapes = DynamicCache.GetShapes(Path, Scale);
            Structure.AddStaticElement(model.Transform(Transformation));
            Structure.AddCollisionShape(shapes.Select(S => S.Transform(Transformation)).ToArray());
            SceneLoader.LoadIfExists(Structure, Path, Scale, Transformation, Settings ?? new SceneSettings());
        }

        private void AddDockModel(CollidableStructure Structure, Random Rng, Vector2 Offset, Vector3 ShorePosition, out Vector2 MainDockPosition, out Vector2 MainDockDirection)
        {
            MainDockDirection = (Offset - ShorePosition.Xz).ToVector3().NormalizedFast().Xz;
            var position = ShorePosition + Structure.Mountain.Radius * .65f * MainDockDirection.ToVector3();
            
            const int maxDocks = 3;
            var dockNumber = Rng.Next(0, maxDocks);
            var basePath = $"Assets/Env/Structures/FishingSettlement/FishingDock{dockNumber}";
            var model = DynamicCache.Get($"{basePath}-Mesh.ply", FishingPostScale);
            var euler = Physics.DirectionToEuler(MainDockDirection.ToVector3());
            var transformationMatrix = Matrix4.CreateRotationY(euler.Y * Mathf.Radian);
            transformationMatrix *= Matrix4.CreateTranslation(position + Structure.Position.Y * Vector3.UnitY);
            model.Transform(transformationMatrix);

            var shapes = DynamicCache.GetShapes($"{basePath}.ply", FishingPostScale);
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
            Structure.AddGroundwork(new LineGroundwork(Structure.Position.Xz, position.Xz + MainDockDirection * Chunk.BlockSize * 4f)
            {
                Width = 16,
                Type = PathType,
                BonusHeight = -0.05f
            });
            MainDockPosition = position.Xz;
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
            return (Biome.Generation.GetMaxHeight(TargetPosition.X, TargetPosition.Z) < BiomePool.SeaLevel - 1f);
        }
        
        private static Vector3 FishingPostScale => Vector3.One * 11f;
    }
}