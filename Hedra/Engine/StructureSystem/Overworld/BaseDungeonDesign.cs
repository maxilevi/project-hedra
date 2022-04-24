using System;
using System.Numerics;
using BulletSharp.Math;
using Hedra.AISystem;
using Hedra.AISystem.Behaviours;
using Hedra.Components;
using Hedra.Engine.Generation;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.Sound;
using Vector3 = System.Numerics.Vector3;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public abstract class BaseDungeonDesign<T> : SimpleCompletableStructureDesign<T>
        where T : BaseStructure, ICompletableStructure
    {
        public override bool CanSpawnInside => false;
        public override string DisplayName => Translations.Get("structure_dungeon");
        protected override BlockType PathType => BlockType.StonePath;
        protected abstract int Level { get; }

        public override int[] AmbientSongs { get; } =
        {
            SoundtrackManager.FacingTheBeast,
            SoundtrackManager.SkeletonSkirmish
        };

        protected override string GetDescription(T Structure)
        {
            throw new NotImplementedException();
        }

        protected override string GetShortDescription(T Structure)
        {
            throw new NotImplementedException();
        }

        protected override float BuildRotationAngle(Random Rng)
        {
            return Rng.Next(0, 4) * 90;
        }

        protected Lever AddLever(CollidableStructure Structure, Vector3 Position, Matrix4x4 Rotation)
        {
            var lever = new Lever(
                Vector3.Transform((Position + StructureOffset) * StructureScale, Rotation) + Structure.Position,
                StructureScale);
            var axisAngle = Rotation.ExtractRotation().ToAxisAngle();
            lever.Rotation = axisAngle.Xyz() * axisAngle.W * Mathf.Degree;
            lever.Condition = () => StructureContentHelper.IsNotNearEnemies(lever.Position);
            Structure.WorldObject.AddChildren(lever);
            return lever;
        }

        protected static BaseStructure BuildDungeonDoorTrigger(Vector3 Point, VertexData Mesh)
        {
            return new DungeonDoorTrigger(Point, Mesh);
        }

        protected static BaseStructure BuildBossRoomTrigger(Vector3 Point, VertexData Mesh)
        {
            return new DungeonBossRoomTrigger(Point, Mesh);
        }

        protected static void AddImmuneTag(IEntity Skeleton)
        {
            Skeleton.AddComponent(new IsStructureMemberComponent(Skeleton));
            Skeleton.SearchComponent<DamageComponent>()
                .Ignore(E => E.SearchComponent<IsStructureMemberComponent>() != null);
            Skeleton.SearchComponent<IBehaviouralAI>()
                .AlterBehaviour<RoamBehaviour>(new DungeonRoamBehaviour(Skeleton));
        }
    }
}