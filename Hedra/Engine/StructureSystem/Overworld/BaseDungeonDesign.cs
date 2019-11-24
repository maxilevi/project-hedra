using System;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using Hedra.AISystem;
using Hedra.AISystem.Behaviours;
using Hedra.AISystem.Humanoid;
using Hedra.API;
using Hedra.Components;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Scenes;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Items;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public abstract class BaseDungeonDesign<T> : SimpleCompletableStructureDesign<T> where T : BaseStructure, ICompletableStructure
    {
        public override bool CanSpawnInside => false;
        public override string DisplayName => Translations.Get("structure_dungeon");
        protected override BlockType PathType => BlockType.StonePath;
        protected abstract int Level { get; }
        
        protected override string GetDescription(T Structure) => throw new System.NotImplementedException();

        protected override string GetShortDescription(T Structure) => throw new System.NotImplementedException();

        protected Lever AddLever(CollidableStructure Structure, Vector3 Position, Matrix4x4 Rotation)
        {
            var lever = new Lever(Vector3.Transform((Position + StructureOffset) * StructureScale, Rotation) + Structure.Position, StructureScale);
            var axisAngle = Rotation.ExtractRotation().ToAxisAngle();
            lever.Rotation = axisAngle.Xyz() * axisAngle.W * Mathf.Degree;
            lever.Condition = () => IsNearEnemies(lever.Position);
            Structure.WorldObject.AddChildren(lever);
            return lever;
        }
        
        private static bool IsNearEnemies(Vector3 Position)
        {
            var mobs = World.Entities;
            return mobs.Count(M => M.Distance(Position) < 32 && M.SearchComponent<IsDungeonMemberComponent>() != null) == 0;
        }

        protected static Chest AddRewardChest(Vector3 Position, VertexData Model)
        {
            var chest = World.SpawnChest(Position, ItemPool.Grab(Utils.Rng.Next(0, 5) == 1 ? ItemTier.Rare : ItemTier.Uncommon));
            chest.Condition = () => IsNearEnemies(chest.Position);
            var triangle = Model.Vertices;
            var direction = Vector3.Zero;
            for (var h = 0; h < 3; ++h)
            {
                var i = h;
                var j = (h + 1) % 3;
                var k = (h + 2) % 3;
                var ij = (triangle[i] - triangle[j]).LengthFast();
                var ik = (triangle[i] - triangle[k]).LengthFast();
                var jk = (triangle[k] - triangle[j]).LengthFast();
                if (ij < ik && ij < jk)
                {
                    var avg = (triangle[i] + triangle[j]) / 2;
                    direction = (avg - triangle[k]).NormalizedFast();
                    break;
                }
            }

            chest.Rotation = Physics.DirectionToEuler(direction) + 90 * Vector3.UnitY;
            return chest;
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
            Skeleton.AddComponent(new IsDungeonMemberComponent(Skeleton));
            Skeleton.SearchComponent<DamageComponent>().Ignore(E => E.SearchComponent<IsDungeonMemberComponent>() != null);
            Skeleton.SearchComponent<IBehaviouralAI>().AlterBehaviour<RoamBehaviour>(new DungeonRoamBehaviour(Skeleton));
        }
    }
}