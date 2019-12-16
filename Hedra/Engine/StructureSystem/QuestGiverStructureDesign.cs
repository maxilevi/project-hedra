using System;
using System.Linq;
using Hedra.BiomeSystem;
using Hedra.Core;
using Hedra.Engine.BiomeSystem;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Mission;
using System.Numerics;
using Hedra.Numerics;

namespace Hedra.Engine.StructureSystem
{
    public abstract class QuestGiverStructureDesign<T> : SimpleStructureDesign<T> where T : BaseStructure, IQuestStructure
    {
        protected abstract Vector3 NPCOffset { get; }
        protected abstract float QuestChance { get; }
        protected virtual Vector3 DefaultLookingDirection => Vector3.UnitZ;
        
        protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation, Random Rng)
        {
            if (Rng.NextFloat() < QuestChance)
            {
                var position = Vector3.Transform(Vector3.Transform(NPCOffset, Rotation), Rotation * Translation);
                var quest = SelectQuest(position, Rng);
                DoWhenChunkReady(position, P =>
                {
                    var npc = NPCCreator.SpawnQuestGiver(P, quest, Rng);
                    npc.Rotation = Physics.DirectionToEuler(npc.Orientation = -Vector3.Transform(DefaultLookingDirection, Rotation));
                    npc.Position = P;
                    npc.Physics.UsePhysics = false;
                    ((T) Structure.WorldObject).NPC = npc;
                }, Structure);
            }
        }

        protected virtual IMissionDesign SelectQuest(Vector3 Position, Random Rng)
        {
            return MissionPool.Random(Position);
        }
    }
}