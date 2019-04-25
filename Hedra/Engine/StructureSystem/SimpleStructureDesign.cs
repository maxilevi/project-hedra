using System;
using System.Drawing.Drawing2D;
using System.Linq;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Generation;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public abstract class SimpleStructureDesign<T> : StructureDesign where T : BaseStructure
    {
        protected virtual Vector3 Scale => Vector3.One;
        protected virtual float EffectivePlateauRadius => PlateauRadius;
        protected abstract int StructureChance { get; }
        protected virtual BlockType PathType => BlockType.StonePath;
        protected abstract CacheItem? Cache { get; }
        
        public sealed override void Build(CollidableStructure Structure)
        {
            var originalModel = Cache != null ? CacheManager.GetModel(Cache.Value) : null;
            var rng = BuildRng(Structure);
            var rotation = Matrix4.CreateRotationY(Mathf.Radian * rng.NextFloat() * 360f);
            var translation = Matrix4.CreateTranslation(Structure.Position);
            var transformation = Matrix4.CreateScale(Scale) * rotation * translation;
            if (originalModel != null)
            {
                Structure.AddStaticElement(
                    originalModel.Clone().Transform(transformation)
                );
                Structure.AddCollisionShape(
                    CacheManager.GetShape(originalModel).DeepClone().Select(S => S.Transform(transformation)).ToArray()
                );
            }

            DoBuild(Structure, rotation, translation, rng);
        }

        protected virtual void DoBuild(CollidableStructure Structure, Matrix4 Rotation, Matrix4 Translation, Random Rng)
        {
        }
        
        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            var structure = base.Setup(TargetPosition, Rng, Create(TargetPosition, EffectivePlateauRadius));
            structure.AddGroundwork(new RoundedGroundwork(TargetPosition, EffectivePlateauRadius / 2, PathType));
            structure.Mountain.Radius = EffectivePlateauRadius;
            return structure;
        }

        protected override bool SetupRequirements(Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng)
        {
            return Rng.Next(0, StructureChance) == 1 &&
                   Biome.Generation.GetHeight(TargetPosition.X, TargetPosition.Z, null, out _) > BiomePool.SeaLevel &&
                   Math.Abs(LandscapeGenerator.River(TargetPosition.Xz)) < 0.005f;
        }
        
        protected abstract T Create(Vector3 Position, float Size);
    }
}