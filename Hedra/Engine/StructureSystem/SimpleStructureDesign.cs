using System;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Linq;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Core;
using Hedra.Engine.Generation;
using Hedra.Engine.PlantSystem;
using Hedra.Engine.PlantSystem.Harvestables;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;
using System.Numerics;
using Hedra.Framework;
using Hedra.Numerics;

namespace Hedra.Engine.StructureSystem
{
    public abstract class SimpleStructureDesign<T> : StructureDesign where T : BaseStructure
    {
        protected virtual Vector3 StructureScale => Vector3.One;
        protected virtual Vector3 StructureOffset => Vector3.Zero;
        protected virtual float EffectivePlateauRadius => PlateauRadius;
        protected virtual float GroundworkRadius => EffectivePlateauRadius / 2;
        protected abstract int StructureChance { get; }
        protected virtual BlockType PathType => BlockType.StonePath;
        protected virtual bool NoPlantsZone { get; }
        protected abstract CacheItem? Cache { get; }
        
        public sealed override void Build(CollidableStructure Structure)
        {
            var originalModel = Cache != null ? CacheManager.GetModel(Cache.Value) : null;
            var rng = BuildRng(Structure);
            var rotation = Matrix4x4.CreateRotationY(Mathf.Radian * BuildRotationAngle(rng));
            var translation = Matrix4x4.CreateTranslation(Structure.Position + StructureOffset);
            var transformation = Matrix4x4.CreateScale(StructureScale) * rotation * translation;
            if (originalModel != null)
            {
                var modelClone = originalModel.Clone();
                ApplyColors(modelClone, World.BiomePool.GetAverageRegionColor(translation.ExtractTranslation()));
                Structure.AddStaticElement(
                    modelClone.Transform(transformation)
                );
                Structure.AddCollisionShape(
                    CacheManager.GetShape(originalModel).DeepClone().Select(S => S.Transform(transformation)).ToArray()
                );
            }

            DoBuild(Structure, rotation, translation, rng);
        }

        protected virtual void ApplyColors(VertexData Model, RegionColor Colors)
        {
            
        }

        protected virtual float BuildRotationAngle(Random Rng)
        {
            return Rng.NextFloat() * 360f;
        }
        
        protected virtual void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation, Random Rng)
        { 
        }
        
        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            var structure = base.Setup(TargetPosition, Rng, Create(TargetPosition, EffectivePlateauRadius));
            structure.AddGroundwork(new RoundedGroundwork(TargetPosition, GroundworkRadius, PathType)
            {
                NoPlants = NoPlantsZone
            });
            structure.Mountain.Radius = EffectivePlateauRadius;
            return structure;
        }

        protected override bool SetupRequirements(ref Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome, IRandom Rng)
        {
            Debug.Assert(StructureChance != 1);
            return Rng.Next(0, StructureChance) == 1 &&
                   Biome.Generation.GetMaxHeight(TargetPosition.X, TargetPosition.Z) > BiomePool.SeaLevel &&
                   Math.Abs(Biome.Generation.RiverAtPoint(TargetPosition.X, TargetPosition.Z)) < 0.005f;
        }

        protected void AddDoor(VertexData Model, Vector3 DoorPosition, Matrix4x4 Transformation, CollidableStructure Structure, bool InvertedRotation, bool InvertedPivot)
        {
            Structure.WorldObject.AddChildren(
                Builder<IBuildingParameters>.CreateDoor(
                    Model,
                    Structure.Position,
                    DoorPosition + StructureOffset,
                    StructureScale,
                    Transformation,
                    Structure,
                    InvertedRotation,
                    InvertedPivot
                )
            );
        }

        protected void AddPlant(IAllocator Allocator, Vector3 Position, HarvestableDesign Design, Random Rng)
        {
            Position += StructureOffset;
            World.EnvironmentGenerator.GeneratePlant(
                Allocator,
                Position,
                World.GetRegion(Position),
                Design,
                Matrix4x4.CreateScale(Design.Scale(Rng)) * Matrix4x4.CreateRotationY(Rng.NextFloat() * 360f) * Matrix4x4.CreateTranslation(Position)
            );
        }
        
        protected abstract T Create(Vector3 Position, float Size);
    }
}