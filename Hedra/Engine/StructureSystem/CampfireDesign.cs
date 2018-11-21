using System;
using System.Collections.Generic;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ComplexMath;
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
        private const int Level = 6;
        public override int Radius { get; set; } = 80;
        public override VertexData Icon => null;
        public override int[] AmbientSongs { get; } =
        {
            SoundtrackManager.OnTheLam
        };

        public override void Build(CollidableStructure Structure)
        {
            var position = Structure.Position;
            var rng = new Random((int) (position.X / 11 * (position.Z / 13)));
            var originalCampfire = CacheManager.GetModel(CacheItem.Campfire);
            var model = originalCampfire.ShallowClone();

            var rotation = rng.NextFloat() * 360.0f;
            var transMatrix = Matrix4.CreateScale(3 + rng.NextFloat() * 1.5f);
            var rotMat = Matrix4.CreateRotationY(rotation);
            transMatrix *= rotMat;
            transMatrix *= Matrix4.CreateTranslation(position);
            model.Transform(transMatrix);
            model.Color(AssetManager.ColorCode1, Utils.VariateColor(TentColor(rng), 15, rng));

            var shapes = CacheManager.GetShape(originalCampfire).DeepClone();
            for (var i = 0; i < shapes.Count; i++)
            {
                shapes[i].Transform(transMatrix);
            }

            Structure.AddStaticElement(model);
            Structure.AddCollisionShape(shapes.ToArray());

            ((Campfire) Structure.WorldObject).Bandit =
                World.WorldBuilding.SpawnBandit(new Vector3(position.X, 125, position.Z), Level);

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
                var pad = new SleepingPad(position + padOffset)
                {
                    TargetRotation = rotation * Vector3.UnitY
                };
                Structure.WorldObject.AddChildren(pad);
            }
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            var structure = base.Setup(TargetPosition, Rng, new Campfire(TargetPosition));
            structure.Mountain.Radius = 48;
            return structure;
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng)
        {
            float height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out _);

            return Rng.Next(0, 12) == 1 && height > BiomePool.SeaLevel;
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