using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Hedra.AISystem;
using Hedra.AISystem.Behaviours;
using Hedra.BiomeSystem;
using Hedra.Components;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.PlantSystem.Harvestables;
using Hedra.Engine.Scenes;
using Hedra.Engine.StructureSystem.Overworld;
using Hedra.Engine.StructureSystem.VillageSystem;
using Hedra.Engine.StructureSystem.VillageSystem.Builders;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Framework;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem
{
    public abstract class SimpleStructureDesign<T> : StructureDesign where T : BaseStructure
    {
        protected virtual Vector3 StructureScale => Vector3.One;
        protected virtual Vector3 StructureOffset => Vector3.Zero;
        protected virtual float EffectivePlateauRadius => PlateauRadius;
        protected virtual float GroundworkRadius => EffectivePlateauRadius / 2;
        protected virtual BlockType PathType => BlockType.StonePath;
        protected virtual bool NoPlantsZone { get; }
        protected virtual bool NoTreesZone { get; }
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

        protected virtual void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation,
            Random Rng)
        {
        }

        protected override CollidableStructure Setup(Vector3 TargetPosition, Random Rng)
        {
            var structure = base.Setup(TargetPosition, Rng, Create(TargetPosition, EffectivePlateauRadius));
            structure.AddGroundwork(new RoundedGroundwork(TargetPosition, GroundworkRadius, PathType)
            {
                NoPlants = NoPlantsZone,
                NoTrees = NoTreesZone
            });
            structure.Mountain.Radius = EffectivePlateauRadius;
            return structure;
        }

        protected override bool SetupRequirements(ref Vector3 TargetPosition, Vector2 ChunkOffset, Region Biome,
            IRandom Rng)
        {
            return InWater(TargetPosition, Biome);
        }

        protected Door AddDoor(VertexData Model, Vector3 DoorPosition, Matrix4x4 RotationMatrix,
            CollidableStructure Structure, bool InvertedRotation, bool InvertedPivot)
        {
            var door = Builder<IBuildingParameters>.CreateDoor(
                Model,
                Structure.Position,
                DoorPosition + StructureOffset,
                StructureScale,
                RotationMatrix,
                Structure,
                InvertedRotation,
                InvertedPivot
            );
            Structure.WorldObject.AddChildren(
                door
            );
            return door;
        }
        
        protected Door AddDoor(VertexData Model, Matrix4x4 RotationMatrix,
            CollidableStructure Structure, bool InvertedRotation, bool InvertedPivot)
        {
            var calculatedDoorPosition = Model.AverageVertices();
            return AddDoor(Model, calculatedDoorPosition, RotationMatrix, Structure, InvertedRotation, InvertedPivot);
        }

        protected static void AddImmuneTag(IEntity Bandit)
        {
            Bandit.AddComponent(new IsStructureMemberComponent(Bandit));
            Bandit.SearchComponent<DamageComponent>()
                .Ignore(E => E.SearchComponent<IsStructureMemberComponent>() != null);
            Bandit.SearchComponent<IBehaviouralAI>().AlterBehaviour<RoamBehaviour>(new DungeonRoamBehaviour(Bandit));
        }

        protected static void AddPlant(IAllocator Allocator, Vector3 Position, HarvestableDesign Design, Random Rng)
        {
            World.EnvironmentGenerator.GeneratePlant(
                Allocator,
                Position,
                World.GetRegion(Position),
                Design,
                Matrix4x4.CreateScale(Design.Scale(Rng)) * Matrix4x4.CreateRotationY(Rng.NextFloat() * 360f) *
                Matrix4x4.CreateTranslation(Position)
            );
        }

        protected abstract T Create(Vector3 Position, float Size);
    }
}