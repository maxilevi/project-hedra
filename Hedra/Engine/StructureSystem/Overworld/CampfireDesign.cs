using System;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Generation;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;
using Hedra.Sound;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class CampfireDesign : StructureDesign
    {
        private const int Level = 6;
        public const int MaxRadius = 80;
        public override int PlateauRadius { get; } = 80;
        public override VertexData Icon => null;

        public override void Build(CollidableStructure Structure)
        {
            var rng = BuildRng(Structure);
            var rotation = rng.NextFloat() * 360.0f * Vector3.UnitY;
            BuildBaseCampfire(Structure, rotation, rng, out var transformationMatrix);

            ((Campfire) Structure.WorldObject).Bandit =
                    World.WorldBuilding.SpawnBandit(
                        new Vector3(Structure.Position.X, 125, Structure.Position.Z) 
                        + Vector3.TransformPosition(Vector3.UnitZ * -12f, Matrix4.CreateRotationY(rotation.Y * Mathf.Radian)),
                        Level
                    );

            ((Campfire) Structure.WorldObject).Bandit.IsSitting = true;
            if (rng.Next(0, 5) != 1)
            {
                SpawnMat(
                    Vector3.TransformPosition(Vector3.UnitX * -12f, Matrix4.CreateRotationY(rotation.Y * Mathf.Radian)),
                    rotation,
                    transformationMatrix,
                    Structure
                    );
            }
        }
        
        protected static void BuildBaseCampfire(CollidableStructure Structure, Vector3 Rotation, Random Rng, out Matrix4 TransformationMatrix)
        {
            var originalCampfire = CacheManager.GetModel(CacheItem.Campfire);
            var model = originalCampfire.ShallowClone();

            TransformationMatrix = Matrix4.CreateScale(3 + Rng.NextFloat() * 1.5f);
            var rotMat = Matrix4.CreateRotationY(Rotation.Y * Mathf.Radian);
            TransformationMatrix *= rotMat;
            TransformationMatrix *= Matrix4.CreateTranslation(Structure.Position);
            model.Transform(TransformationMatrix);
            model.Color(AssetManager.ColorCode1, Utils.VariateColor(TentColor(Rng), 15, Rng));

            var shapes = CacheManager.GetShape(originalCampfire).DeepClone();
            for (var i = 0; i < shapes.Count; i++)
            {
                shapes[i].Transform(TransformationMatrix);
            }

            Structure.AddStaticElement(model);
            Structure.AddCollisionShape(shapes.ToArray());
        }

        protected static void SpawnMat(Vector3 Position, Vector3 CampfireRotation, Matrix4 TransformationMatrix,
            CollidableStructure Structure)
        {
            var padOffset = Position + Vector3.UnitZ * -1f;
            var originalModel = CacheManager.GetModel(CacheItem.Mat);
            var model = originalModel.Clone();
            model.Scale(Vector3.One * .75f);
            model.Transform(TransformationMatrix);
            model.Translate(padOffset);

            var shapes = CacheManager.GetShape(originalModel).DeepClone();
            for (var i = 0; i < shapes.Count; i++)
            {
                shapes[i].Transform(Matrix4.CreateScale(Vector3.One * .75f));
                shapes[i].Transform(TransformationMatrix);
                shapes[i].Transform(padOffset);
                Structure.AddCollisionShape(shapes[i]);
            }

            Structure.AddStaticElement(model);
            var pad = new SleepingPad(Position + Structure.Position + Vector3.UnitY)
            {
                TargetRotation = CampfireRotation
            };
            Structure.WorldObject.AddChildren(pad);
        }
        
        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            var structure = base.Setup(TargetPosition, Rng, new Campfire(TargetPosition));
            structure.Mountain.Radius = 48;
            structure.AddGroundwork(new RoundedGroundwork(TargetPosition, 24, BlockType.Dirt));
            return structure;
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng)
        {
            var height = Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out _);

            return Rng.Next(0, StructureGrid.CampfireChance) == 1 && height > BiomePool.SeaLevel && Math.Abs(LandscapeGenerator.River(TargetPosition.Xz)) < 0.005f;
        }

        public static Vector4 TentColor(Random Rng)
        {
            var n = Rng.Next(0, 5);
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