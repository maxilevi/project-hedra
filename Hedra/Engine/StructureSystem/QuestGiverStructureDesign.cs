using System;
using System.Linq;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.ComplexMath;
using Hedra.Engine.Generation;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem
{
    public abstract class QuestGiverStructureDesign<T> : StructureDesign where T : BaseStructure, IQuestStructure
    {
        protected abstract float EffectivePlateauRadius { get; }
        protected abstract int StructureChance { get; }
        protected virtual BlockType PathType => BlockType.StonePath;
        protected abstract CacheItem Cache { get; }
        protected abstract Vector3 Offset { get; }
        protected abstract float QuestChance { get; }
        protected virtual Vector3 DefaultLookingDirection => Vector3.UnitZ;
        
        public override void Build(CollidableStructure Structure)
        {
            var originalModel = CacheManager.GetModel(Cache);
            var rng = BuildRng(Structure);
            var rotation = Matrix4.CreateRotationY(Mathf.Radian * rng.NextFloat() * 360f);
            var transformation = rotation *  Matrix4.CreateTranslation(Structure.Position);

            if (rng.NextFloat() < QuestChance)
            {
                DoWhenChunkReady(Vector3.TransformPosition(Vector3.Zero, transformation), P =>
                {
                    var position = Vector3.TransformPosition(Offset, rotation) + P;
                    var npc = World.WorldBuilding.SpawnVillager(
                        position,
                        rng
                    );
                    npc.Rotation = Physics.DirectionToEuler(npc.Orientation = -Vector3.TransformPosition(DefaultLookingDirection, rotation));
                    npc.Physics.TargetPosition = position;
                    npc.Physics.UsePhysics = false;
                    npc.AddComponent(new QuestGiverComponent(npc, QuestPool.Grab().Build(npc.Position, Utils.Rng, npc)));
                    ((T) Structure.WorldObject).NPC = npc;
                }, Structure);
            }
            Structure.AddStaticElement(
                originalModel.Clone().Transform(transformation)
            );
            Structure.AddCollisionShape(
                CacheManager.GetShape(originalModel).DeepClone().Select(S => S.Transform(transformation)).ToArray()
            );           
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