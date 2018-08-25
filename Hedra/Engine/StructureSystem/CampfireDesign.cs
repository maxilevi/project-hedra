using System;
using System.Collections.Generic;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class CampfireDesign : StructureDesign
    {
        public override int Radius { get; set; } = 80;
        public override VertexData Icon => null;

        public override void Build(Vector3 Position, CollidableStructure Structure)
        {
            var underChunk = World.GetChunkAt(Position);
            var rng = new Random((int) (Position.X / 11 * (Position.Z / 13)));
            var originalCampfire = CacheManager.GetModel(CacheItem.Campfire);
            var model = originalCampfire.ShallowClone();

            var rotation = rng.NextFloat() * 360.0f;
            Matrix4 transMatrix = Matrix4.CreateScale(3 + rng.NextFloat() * 1.5f);
            Matrix4 rotMat = Matrix4.CreateRotationY(rotation);
            transMatrix *= rotMat;
            transMatrix *= Matrix4.CreateTranslation(Position);
            model.Transform(transMatrix);
            model.Color(AssetManager.ColorCode1, Utils.VariateColor(TentColor(rng), 15, rng));

            var shapes = CacheManager.GetShape(originalCampfire).DeepClone();
            for (int i = 0; i < shapes.Count; i++)
            {
                shapes[i].Transform(transMatrix);
            }

            Structure.AddStaticElement(model);
            Structure.AddCollisionShape(shapes.ToArray());
            
            var fire = new Campfire(Position);
            Executer.ExecuteOnMainThread(
                () => World.WorldBuilding.SpawnBandit(new Vector3(Position.X, 125, Position.Z), false, false)
            );
            World.AddStructure(fire);

            if (rng.Next(0, 5) != 1)
            {
                var padOffset = Vector3.TransformPosition(Vector3.UnitX * -12f, rotMat) + Vector3.UnitY * .25f;
                var originalModel = CacheManager.GetModel(CacheItem.Mat);
                model = originalModel.Clone();
                model.Scale(Vector3.One * .8f);
                model.Transform(transMatrix);
                model.Translate(padOffset);

                shapes = CacheManager.GetShape(originalModel).DeepClone();
                for (var i = 0; i < shapes.Count; i++)
                {
                    shapes[i].Transform(Matrix4.CreateScale(Vector3.One * .8f));
                    shapes[i].Transform(transMatrix);
                    shapes[i].Transform(padOffset);
                    Structure.AddCollisionShape(shapes[i]);
                }

                Structure.AddStaticElement(model);
                var pad = new SleepingPad(Position + padOffset)
                {
                    TargetRotation = rotation * Vector3.UnitY
                };
                World.AddStructure(pad);
                UpdateManager.Add(pad);
            }
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Vector2 NewOffset, Region Biome, Random Rng)
        {
            var plateau = new Plateau(TargetPosition, 48);
            World.WorldBuilding.AddPlateau(plateau);
            return new CollidableStructure(this, TargetPosition, plateau);
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, Random Rng)
        {
            float height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out _);

            return Rng.Next(0, 12) == 1 && height > 0;
        }


        public static Vector4 TentColor(Random Rng)
        {
            int n = Rng.Next(0, 5);
            switch (n)
            {
                case 0: return Colors.FromHtml("#92A86D");
                case 1: return Colors.FromHtml("#4E4E4E");
                case 2: return Colors.FromHtml("#6E4F40");
                case 3: return Colors.FromHtml("#C9D976");
                case 4: return Colors.FromHtml("#D46872");
                default: return Colors.FromHtml("#92A86D");
            }
        }
    }
}