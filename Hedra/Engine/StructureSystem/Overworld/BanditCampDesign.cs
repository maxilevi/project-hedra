using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.WorldBuilding;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.Sound;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class BanditCampDesign : CompletableStructureDesign<BanditCamp>
    {
        private const int Level = 18;
        public override int PlateauRadius { get; } = 328;
        public override VertexData Icon { get; } = CacheManager.GetModel(CacheItem.CampfireIcon);
        public override int[] AmbientSongs { get; } =
        {
            SoundtrackManager.HostageSituation
        };

        public override void Build(CollidableStructure Structure)
        {
            const float roasterScale = .75f;
            var position = Structure.Position;
            var rng = new Random((int)(position.X / 11 * (position.Z / 13)));
            var originalRoaster = CacheManager.GetModel(CacheItem.CampfireRoaster);
            var roasterModel = originalRoaster.ShallowClone();
            roasterModel.Transform(Matrix4x4.CreateScale(roasterScale));
            var originalCenterModel = CacheManager.GetModel(CacheItem.CampfireLogs);
            var centerModel = originalCenterModel.ShallowClone();
            var model = new VertexData();

            var scaleMatrix = Structure.Parameters.Get<Matrix4x4>("ScaleMatrix");
            
            roasterModel.Transform(scaleMatrix);
            centerModel.Transform(scaleMatrix);

            model += roasterModel;
            model += centerModel;

            var fireShapes = CacheManager.GetShape(originalCenterModel).DeepClone();
            var roasterShapes = CacheManager.GetShape(originalRoaster).DeepClone();
            var shapes = new List<CollisionShape>
            {
                fireShapes[0]
            };

            shapes.AddRange(roasterShapes.ToArray());
            for (var i = 0; i < shapes.Count; i++)
            {
                if (i != 0) shapes[i].Transform(Matrix4x4.CreateScale(roasterScale));
                shapes[i].Transform(scaleMatrix);
            }

            var tents = Structure.Parameters.Get<TentParameters[]>("TentParameters");
            var enemies = new Entity[tents.Length];

            for (var i = 0; i < tents.Length; i++)
            {
                MakeTent(tents[i], rng, Structure);
                enemies[i] = World.WorldBuilding.SpawnBandit(
                    tents[i].WorldPosition + Vector3.Transform(Vector3.UnitZ * 24, tents[i].RotationMatrix), Level);
            }

            DecorationsPlacer.PlaceWhenWorldReady(position, P =>
            {
                var transform = Matrix4x4.CreateTranslation(P);
                Structure.AddCollisionShape(shapes.Select(S => S.Transform(transform)).ToArray());
                Structure.AddStaticElement(model.Transform(transform)); 
            }, () => Structure.Disposed);
            ((BanditCamp) Structure.WorldObject).Enemies = enemies;
        }

        private bool IntersectsWithOtherCampfires(List<TentParameters> OccupiedSpots, Vector3 NewSpot)
        {
            for (var i = 0; i < OccupiedSpots.Count; i++)
            {
                if ((OccupiedSpots[i].WorldPosition - NewSpot).LengthSquared() < 16*16) return true;
            }
            return false;
        }

        private static void MakeTent(TentParameters Parameters, Random Rng, CollidableStructure Structure)
        {
            RoutineManager.StartRoutine(TentCoroutine, Parameters, Rng, Structure);
        }

        private static IEnumerator TentCoroutine(object[] Params)
        {
            var currentModelOffset = -Vector3.UnitX * 4f;
            var parameters = (TentParameters) Params[0];
            var rng = (Random)Params[1];
            var structure = (CollidableStructure) Params[2];
            Chunk underChunk = null;
            var currentSeed = World.Seed;
            while (underChunk?.Landscape == null || !underChunk.Landscape.StructuresPlaced)
            {
                if(World.Seed != currentSeed || structure.Disposed) yield break;
                underChunk = World.GetChunkAt(parameters.WorldPosition);
                yield return null;
            }

            TaskScheduler.Parallel(delegate
            {
                parameters.Position = new Vector3(
                    parameters.Position.X,
                    Physics.HeightAtPosition(parameters.Position),
                    parameters.Position.Z
                );
                parameters.Position = new Vector3(
                    parameters.WorldPosition.X,
                    Physics.HeightAtPosition(parameters.WorldPosition),
                    parameters.WorldPosition.Z
                );
                
                var originalCampfire = CacheManager.GetModel(CacheItem.CampfireTent);
                var campfireShapes = CacheManager.GetShape(originalCampfire).DeepClone();

                var positionMatrix = Matrix4x4.CreateTranslation(parameters.Position);

                for (var k = 0; k < campfireShapes.Count; k++)
                {
                    campfireShapes[k].Transform(Matrix4x4.CreateTranslation(currentModelOffset));
                    campfireShapes[k].Transform(parameters.TransformationMatrix);
                    campfireShapes[k].Transform(parameters.RotationMatrix);
                    campfireShapes[k].Transform(positionMatrix);
                }

                var campfire = originalCampfire.ShallowClone();
                campfire.Transform(Matrix4x4.CreateTranslation(currentModelOffset));
                campfire.Transform(parameters.TransformationMatrix);
                campfire.Transform(parameters.RotationMatrix);
                campfire.Transform(positionMatrix);
                campfire.Color(AssetManager.ColorCode1, Utils.VariateColor(CampfireDesign.TentColor(rng), 15, rng));

                structure.AddCollisionShape(campfireShapes.ToArray());
                structure.AddStaticElement(campfire);
            });
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            var structure = base.Setup(TargetPosition, Rng, new BanditCamp(TargetPosition, this.PlateauRadius));
            var scaleMatrix = Matrix4x4.CreateScale(3 + Rng.NextFloat() * 1.5f);
            var tents = this.SetupTents(TargetPosition, scaleMatrix, Rng);         

            structure.AddGroundwork(new RoundedGroundwork(TargetPosition, 16f, BlockType.Dirt));
            for (var i = 0; i < tents.Length; i++)
            {
                structure.AddGroundwork(new RoundedGroundwork(tents[i].WorldPosition, 16f, BlockType.Dirt));
            }

            structure.Parameters.Set("TentParameters", tents);
            structure.Parameters.Set("ScaleMatrix", scaleMatrix);
            return structure;
        }

        private TentParameters[] SetupTents(Vector3 TargetPosition, Matrix4x4 ScaleMatrix, Random Rng)
        {
            var tents = new List<TentParameters>();

            var extraCampfires = 3 + Rng.Next(0, 2);
            for (var i = 0; i < extraCampfires; i++)
            {
                float dist = (22 + Rng.NextFloat() * 6f) * Chunk.BlockSize;
                var rotationMatrix = Matrix4x4.CreateRotationY(360f / extraCampfires * i * Mathf.Radian);
                var newPosition = Vector3.Transform(Vector3.UnitX * dist, rotationMatrix) + TargetPosition;
                var tent = new TentParameters
                {
                    Position = newPosition,
                    RotationMatrix = Matrix4x4.CreateRotationY(360f / extraCampfires * i * Mathf.Radian),
                    TransformationMatrix = ScaleMatrix,
                    WorldPosition = newPosition
                };
                tents.Add(tent);
            }

            var randomCampfires = 4;
            for (var i = 0; i < randomCampfires; i++)
            {
                var rotationMatrix = Matrix4x4.CreateRotationY(360f * Rng.NextFloat()* Mathf.Radian);
                var spawnRadius = PlateauRadius * .5f;
                var randomPosition = Vector3.UnitX * (Rng.NextFloat() * spawnRadius * 2f - spawnRadius)
                                     + Vector3.UnitZ * (Rng.NextFloat() * spawnRadius * 2f - spawnRadius);
                var newPosition = TargetPosition + randomPosition;

                if (this.IntersectsWithOtherCampfires(tents, newPosition) || (TargetPosition - newPosition).LengthFast() < 6 * Chunk.BlockSize) continue;

                tents.Add(new TentParameters
                {
                    RotationMatrix = rotationMatrix,
                    WorldPosition = newPosition,
                    Position = newPosition,
                    TransformationMatrix = ScaleMatrix
                });
            }
            return tents.ToArray();
        }

        protected override bool SetupRequirements(ref Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng)
        {
            var height = Biome.Generation.GetMaxHeight(TargetPosition.X, TargetPosition.Z);
            return Rng.Next(0, StructureGrid.BanditCampChance) == 1 && height > BiomePool.SeaLevel;
        }

        public override string DisplayName => Translations.Get("structure_bandit_camp");

        protected override string GetShortDescription(BanditCamp Structure)
        {
            return Translations.Get("quest_complete_structure_short_bandit_camp", Structure.Rescuee.Name.ToUpperInvariant());
        }

        protected override string GetDescription(BanditCamp Structure)
        {
            return Translations.Get("quest_complete_structure_description_bandit_camp", Structure.Rescuee.Name.ToUpperInvariant(), DisplayName, Structure.EnemiesLeft);
        }

        public class TentParameters
        {
            public Vector3 Position { get; set; }
            public Matrix4x4 RotationMatrix { get; set; }
            public Matrix4x4 TransformationMatrix { get; set; }
            public Vector3 WorldPosition { get; set; }
        }
    }
}
