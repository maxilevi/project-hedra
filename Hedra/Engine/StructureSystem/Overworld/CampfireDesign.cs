using System;
using System.Numerics;
using Hedra.BiomeSystem;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class CampfireDesign : StructureDesign
    {
        public override int StructureChance => StructureGrid.CampfireChance;
        private const int Level = 6;
        public const int MaxRadius = 80;
        public override int PlateauRadius { get; } = 80;
        public override VertexData Icon => null;
        public override bool CanSpawnInside => false;

        public override void Build(CollidableStructure Structure)
        {
            var rng = BuildRng(Structure);
            var rotation = rng.NextFloat() * 360.0f * Vector3.UnitY;
            BuildBaseCampfire(Structure.Position, rotation, Structure, rng, out var transformationMatrix);

            var npcPosition = new Vector3(Structure.Position.X, 0, Structure.Position.Z)
                              + Vector3.Transform(Vector3.UnitZ * -12f,
                                  Matrix4x4.CreateRotationY(rotation.Y * Mathf.Radian));
            DoWhenChunkReady(npcPosition,
                P => { ((Campfire)Structure.WorldObject).Bandit = CreateCampfireNPC(Structure, P, rng); }, Structure);
            if (rng.Next(0, 5) != 1)
                SpawnCampfireMat(
                    Vector3.Transform(Vector3.UnitX * -12f, Matrix4x4.CreateRotationY(rotation.Y * Mathf.Radian)),
                    rotation,
                    transformationMatrix,
                    Structure
                );
        }

        protected virtual IHumanoid CreateCampfireNPC(CollidableStructure Structure, Vector3 Position, Random Rng)
        {
            var npc = NPCCreator.SpawnBandit(
                Position,
                Level,
                BanditOptions.Default
            );
            npc.IsSitting = true;
            return npc;
        }

        public static void BuildBaseCampfire(Vector3 Position, Vector3 Rotation, CollidableStructure Structure,
            Random Rng, out Matrix4x4 TransformationMatrix)
        {
            var originalCampfire = CacheManager.GetModel(CacheItem.Campfire);
            var model = originalCampfire.ShallowClone();

            TransformationMatrix = Matrix4x4.CreateScale(3 + Rng.NextFloat() * 1.5f);
            var rotMat = Matrix4x4.CreateRotationY(Rotation.Y * Mathf.Radian);
            TransformationMatrix *= rotMat;
            TransformationMatrix *= Matrix4x4.CreateTranslation(Position);
            model.Transform(TransformationMatrix);
            model.Color(AssetManager.ColorCode1, Utils.VariateColor(TentColor(Rng), 15, Rng));

            var shapes = CacheManager.GetShape(originalCampfire).DeepClone();
            for (var i = 0; i < shapes.Count; i++) shapes[i].Transform(TransformationMatrix);

            Structure.AddStaticElement(model);
            Structure.AddCollisionShape(shapes.ToArray());
        }

        public static void SpawnCampfireMat(Vector3 Position, Vector3 CampfireRotation, Matrix4x4 TransformationMatrix,
            CollidableStructure Structure)
        {
            var padOffset = Position + Vector3.UnitZ * -1f;

            SpawnMat(
                Position + Structure.Position,
                CampfireRotation,
                Matrix4x4.CreateScale(Vector3.One * .75f) * TransformationMatrix *
                Matrix4x4.CreateTranslation(padOffset),
                Structure
            );
        }

        public static void SpawnMat(Vector3 Position, Vector3 Rotation, Matrix4x4 TransformationMatrix,
            CollidableStructure Structure)
        {
            var originalModel = CacheManager.GetModel(CacheItem.Mat);
            var model = originalModel.Clone();
            model.Transform(TransformationMatrix);

            var shapes = CacheManager.GetShape(originalModel).DeepClone();
            for (var i = 0; i < shapes.Count; i++)
            {
                shapes[i].Transform(TransformationMatrix);
                Structure.AddCollisionShape(shapes[i]);
            }

            Structure.AddStaticElement(model);
            var pad = new SleepingPad(Position + Vector3.UnitY)
            {
                TargetRotation = Rotation
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

        protected override bool SetupRequirements(ref Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome,
            IRandom Rng)
        {
            return InWater(TargetPosition, Biome);
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