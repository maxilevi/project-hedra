
using System;
using System.Collections;
using System.Collections.Generic;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class BanditCampDesign : StructureDesign
    {
        public override int Radius { get; set; } = 300;
        public override VertexData Icon { get; } = CacheManager.GetModel(CacheItem.CampfireIcon);

        public override void Build(Vector3 Position, CollidableStructure Structure)
        {
            const float roasterScale = .75f;
            var underChunk = World.GetChunkAt(Position);
            var rng = new Random((int)(Position.X / 11 * (Position.Z / 13)));
            var roasterModel = AssetManager.PlyLoader("Assets/Env/Roaster.ply", Vector3.One * roasterScale);
            var centerModel = AssetManager.PlyLoader("Assets/Env/Campfire2.ply", Vector3.One);
            var model = new VertexData();

            var scaleMatrix = Structure.Parameters.Get<Matrix4>("ScaleMatrix");
            var transMatrix = scaleMatrix * Matrix4.CreateTranslation(Position);

            roasterModel.Transform(transMatrix);
            centerModel.Transform(transMatrix);

            model += roasterModel;
            model += centerModel;

            var fireShapes = AssetManager.LoadCollisionShapes("Campfire0.ply", 7, Vector3.One);
            var roasterShapes = AssetManager.LoadCollisionShapes("Roaster0.ply", 5, Vector3.One);
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

            var enemies = new List<Entity>();
            var tents = Structure.Parameters.Get<TentParameters[]>("TentParameters");

            for (var i = 0; i < tents.Length; i++)
            {
                enemies.Add(
                    MakeTent(tents[i], rng)
                );
            }

            Structure.AddCollisionShape(shapes.ToArray());
            underChunk.AddStaticElement(model);
            underChunk.Blocked = true;

            var camp = new BanditCamp(Position, this.Radius)
            {
                Enemies = enemies.ToArray()
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

        private static Humanoid MakeTent(TentParameters Parameters, Random Rng)
        {
            CoroutineManager.StartCoroutine(TentCoroutine, Parameters, Rng);
            return World.QuestManager.SpawnBandit(
                Parameters.Position + new Vector3(Rng.NextFloat() * 16f - 8f, 0, Rng.NextFloat() * 16f - 8f), false, false);
        }

        private static IEnumerator TentCoroutine(object[] Params)
        {
            var currentModelOffset = -Vector3.UnitX * 4f;
            var parameters = (TentParameters) Params[0];
            var rng = (Random)Params[1];
            var underChunk = World.GetChunkAt(parameters.Position);
            while (underChunk == null || !underChunk.Landscape.StructuresPlaced)
            {
                yield return null;
            }
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

            var campfireShapes = AssetManager.LoadCollisionShapes("Campfire0.ply", 7, Vector3.One);
            campfireShapes.RemoveAt(0);

            var positionMatrix = Matrix4.CreateTranslation(parameters.Position);

            for (var k = 0; k < campfireShapes.Count; k++)
            {
                campfireShapes[k].Transform(Matrix4.CreateTranslation(currentModelOffset));
                campfireShapes[k].Transform(parameters.TransformationMatrix);
                campfireShapes[k].Transform(parameters.RotationMatrix);
                campfireShapes[k].Transform(positionMatrix);
            }
            var campfire = AssetManager.PlyLoader("Assets/Env/Campfire1.ply", Vector3.One);
            campfire.Transform(Matrix4.CreateTranslation(currentModelOffset));
            campfire.Transform(parameters.TransformationMatrix);
            campfire.Transform(parameters.RotationMatrix);
            campfire.Transform(positionMatrix);
            campfire.Color(AssetManager.ColorCode1, Utils.VariateColor(CampfireDesign.TentColor(rng), 15, rng));

            underChunk.AddCollisionShape(campfireShapes.ToArray());
            underChunk.AddStaticElement(campfire);
            underChunk.Blocked = true;
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Vector2 NewOffset, Region Biome, Random Rng)
        {
            BlockType type;
            float height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out type);

            var plateau = new Plateau(TargetPosition, Radius, 300f, height);
            World.QuestManager.AddPlateau(plateau);

            var scaleMatrix = Matrix4.CreateScale(3 + Rng.NextFloat() * 1.5f);
            var tents = this.SetupTents(TargetPosition, scaleMatrix, Rng);         

            for (var i = 0; i < tents.Length; i++)
            {
                World.QuestManager.AddVillagePosition(tents[i].WorldPosition, 16f);
            }

            var structure = new CollidableStructure(this, TargetPosition, plateau);
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
            BlockType type;
            float height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out type);
            return Rng.Next(0, 100) == 1 && height > 0;
        }

        internal class TentParameters
        {
            public Vector3 Position { get; set; }
            public Matrix4 RotationMatrix { get; set; }
            public Matrix4 TransformationMatrix { get; set; }
            public Vector3 WorldPosition { get; set; }
        }
    }
}
