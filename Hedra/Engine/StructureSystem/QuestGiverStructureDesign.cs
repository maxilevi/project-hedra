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
using Hedra.Mission;
using System.Numerics;

namespace Hedra.Engine.StructureSystem
{
    public abstract class QuestGiverStructureDesign<T> : SimpleStructureDesign<T> where T : BaseStructure, IQuestStructure
    {
        protected abstract Vector3 Offset { get; }
        protected abstract float QuestChance { get; }
        protected virtual Vector3 DefaultLookingDirection => Vector3.UnitZ;
        
        protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation, Random Rng)
        {
            if (Rng.NextFloat() < QuestChance)
            {
                DoWhenChunkReady(Vector3.Transform(Vector3.Zero, Rotation * Translation), P =>
                {
                    var position = Vector3.Transform(Offset, Rotation) + P;
                    var npc = World.WorldBuilding.SpawnVillager(
                        position,
                        Rng
                    );
                    npc.Rotation = Physics.DirectionToEuler(npc.Orientation = -Vector3.Transform(DefaultLookingDirection, Rotation));
                    npc.Position = position;
                    npc.Physics.UsePhysics = false;
                    npc.AddComponent(new QuestGiverComponent(npc, MissionPool.Random(position)));
                    ((T) Structure.WorldObject).NPC = npc;
                }, Structure);
            }
        }
    }
}