
using System;
using System.Collections;
using System.Collections.Generic;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class BanditCampDesign : StructureDesign
    {
        public override int Radius { get; set; } = 300;
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
            roasterModel.Transform(Matrix4.CreateScale(roasterScale));
            var originalCenterModel = CacheManager.GetModel(CacheItem.CampfireLogs);
            var centerModel = originalCenterModel.ShallowClone();
            var model = new VertexData();

            var scaleMatrix = Structure.Parameters.Get<Matrix4>("ScaleMatrix");
            var transMatrix = scaleMatrix * Matrix4.CreateTranslation(position);

            roasterModel.Transform(transMatrix);
            centerModel.Transform(transMatrix);

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
                if (i != 0) shapes[i].Transform(Matrix4.CreateScale(roasterScale));
                shapes[i].Transform(transMatrix);
            }

            var tents = Structure.Parameters.Get<TentParameters[]>("TentParameters");
            var enemies = new Entity[tents.Length];

            for (var i = 0; i < tents.Length; i++)
            {
                int k = i;
                BanditCampDesign.MakeTent(tents[i], rng, enemies, k, Structure);
            }

            Structure.AddCollisionShape(shapes.ToArray());
            Structure.AddStaticElement(model);

            var camp = new BanditCamp(position, this.Radius)
            {
                Enemies = enemies
            };
            World.AddStructure(camp);
        }

        private bool IntersectsWithOtherCampfires(List<TentParameters> OccupiedSpots, Vector3 NewSpot)
        {
            for (var i = 0; i < OccupiedSpots.Count; i++)
            {
                if ((OccupiedSpots[i].WorldPosition - NewSpot).LengthSquared < 12*12) return true;
            }
            return false;
        }

        private static void MakeTent(TentParameters Parameters, Random Rng, Entity[] Enemies, int K, CollidableStructure Structure)
        {
            CoroutineManager.StartCoroutine(TentCoroutine, Parameters, Rng, Enemies, K, Structure);
        }

        private static IEnumerator TentCoroutine(object[] Params)
        {
            var currentModelOffset = -Vector3.UnitX * 4f;
            var parameters = (TentParameters) Params[0];
            var rng = (Random)Params[1];
            var enemies = (Entity[])Params[2];
            var j = (int) Params[3];
            var structure = (CollidableStructure) Params[4];
            var underChunk = World.GetChunkAt(parameters.Position);
            while (underChunk?.Landscape == null || !underChunk.Landscape.StructuresPlaced)
            {
                yield return null;
            }

            TaskManager.Parallel(delegate
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

                var positionMatrix = Matrix4.CreateTranslation(parameters.Position);

                for (var k = 0; k < campfireShapes.Count; k++)
                {
                    campfireShapes[k].Transform(Matrix4.CreateTranslation(currentModelOffset));
                    campfireShapes[k].Transform(parameters.TransformationMatrix);
                    campfireShapes[k].Transform(parameters.RotationMatrix);
                    campfireShapes[k].Transform(positionMatrix);
                }

                var campfire = originalCampfire.ShallowClone();
                campfire.Transform(Matrix4.CreateTranslation(currentModelOffset));
                campfire.Transform(parameters.TransformationMatrix);
                campfire.Transform(parameters.RotationMatrix);
                campfire.Transform(positionMatrix);
                campfire.Color(AssetManager.ColorCode1, Utils.VariateColor(CampfireDesign.TentColor(rng), 15, rng));

                structure.AddCollisionShape(campfireShapes.ToArray());
                structure.AddStaticElement(campfire);
                enemies[j] = World.WorldBuilding.SpawnBandit(
                    parameters.Position + new Vector3(rng.NextFloat() * 16f - 8f, 0, rng.NextFloat() * 16f - 8f), false,
                    false);
            });
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            var structure = base.Setup(TargetPosition, Rng);
            var scaleMatrix = Matrix4.CreateScale(3 + Rng.NextFloat() * 1.5f);
            var tents = this.SetupTents(TargetPosition, scaleMatrix, Rng);         

            for (var i = 0; i < tents.Length; i++)
            {
                World.WorldBuilding.AddGroundwork(new RoundedGroundwork(tents[i].WorldPosition, 16f));
            }

            structure.Parameters.Set("TentParameters", tents);
            structure.Parameters.Set("ScaleMatrix", scaleMatrix);
            return structure;
        }

        private TentParameters[] SetupTents(Vector3 TargetPosition, Matrix4 ScaleMatrix, Random Rng)
        {
            var tents = new List<TentParameters>();

            var extraCampfires = 3;
            for (var i = 0; i < extraCampfires; i++)
            {
                float dist = 16 + Rng.NextFloat() * 6f;
                var rotationMatrix = Matrix4.CreateRotationY(360f / extraCampfires * i * Mathf.Radian);
                var tent = new TentParameters
                {
                    Position = TargetPosition,
                    RotationMatrix = Matrix4.CreateRotationY(360f / extraCampfires * i * Mathf.Radian),
                    TransformationMatrix = ScaleMatrix * Matrix4.CreateTranslation(Vector3.UnitX * dist),
                    WorldPosition = Vector3.TransformPosition(Vector3.UnitX * dist, rotationMatrix) + TargetPosition
                };
                tents.Add(tent);
            }

            var randomCampfires = 4;
            for (var i = 0; i < randomCampfires; i++)
            {
                var rotationMatrix = Matrix4.CreateRotationY(360f * Rng.NextFloat());
                var spawnRadius = Radius * .5f;
                var randomPosition = Vector3.UnitX * (Rng.NextFloat() * spawnRadius * 2f - spawnRadius)
                                     + Vector3.UnitZ * (Rng.NextFloat() * spawnRadius * 2f - spawnRadius);
                var newPosition = TargetPosition + randomPosition;

                if (this.IntersectsWithOtherCampfires(tents, newPosition)) continue;

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

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, Random Rng)
        {
            var height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out _);
            return Rng.Next(0, 100) == 1 && height > BiomePool.SeaLevel;
        }

        public class TentParameters
        {
            public Vector3 Position { get; set; }
            public Matrix4 RotationMatrix { get; set; }
            public Matrix4 TransformationMatrix { get; set; }
            public Vector3 WorldPosition { get; set; }
        }
    }
}
