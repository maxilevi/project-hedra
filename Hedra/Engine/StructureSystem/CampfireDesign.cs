using System;
using System.Collections.Generic;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public class CampfireDesign : StructureDesign
    {
        public override int Radius { get; set; } = 80;

        public override void Build(Vector3 Position, CollidableStructure Structure)
        {
            var underChunk = World.GetChunkAt(Position);
            var rng = new Random((int) (Position.X / 11 * (Position.Z / 13)));
            VertexData model = CacheManager.GetModel(CacheItem.Campfire).Clone();

            var rotation = rng.NextFloat() * 360.0f;
            Matrix4 transMatrix = Matrix4.CreateScale(3 + rng.NextFloat() * 1.5f);
            Matrix4 rotMat = Matrix4.CreateRotationY(rotation);
            transMatrix *= rotMat;
            transMatrix *= Matrix4.CreateTranslation(Position);
            model.Transform(transMatrix);
            model.Color(AssetManager.ColorCode1, Utils.VariateColor(TentColor(rng), 15, rng));

            List<CollisionShape> shapes = AssetManager.LoadCollisionShapes("Campfire0.ply", 7, Vector3.One);
            for (int i = 0; i < shapes.Count; i++)
            {
                shapes[i].Transform(transMatrix);
                underChunk.AddCollisionShape(shapes[i]);
            }

            underChunk.AddStaticElement(model);

            var fire = new Campfire(Position);
            World.AddStructure(fire);

            if (rng.Next(0, 5) != 1)
            {
                var padOffset = Vector3.TransformPosition(Vector3.UnitX * -12f, rotMat);
                var originalModel = CacheManager.GetModel(CacheItem.Mat);
                model = originalModel.Clone();
                model.Scale(Vector3.One * .8f);
                model.Transform(transMatrix);
                model.Transform(padOffset);

                shapes = CacheManager.GetShape(originalModel).DeepClone();
                for (var i = 0; i < shapes.Count; i++)
                {
                    shapes[i].Transform(Matrix4.CreateScale(Vector3.One * .8f));
                    shapes[i].Transform(transMatrix);
                    shapes[i].Transform(padOffset);
                    underChunk.AddCollisionShape(shapes[i]);
                }

                underChunk.AddStaticElement(model);
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
            BlockType type;
            float height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out type);

            var plateau = new Plateau(TargetPosition, 48, 16, height);
            World.QuestManager.AddPlateau(plateau);
            return new CollidableStructure(this, TargetPosition, plateau);
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, Random Rng)
        {
            BlockType type;
            float height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out type);

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