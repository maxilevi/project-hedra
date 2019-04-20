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
    public abstract class QuestGiverStructureDesign<T> : SimpleStructureDesign<T> where T : BaseStructure, IQuestStructure
    {
        protected abstract Vector3 Offset { get; }
        protected abstract float QuestChance { get; }
        protected virtual Vector3 DefaultLookingDirection => Vector3.UnitZ;
        
        protected override void DoBuild(CollidableStructure Structure, Matrix4 Rotation, Matrix4 Translation, Random Rng)
        {
            if (Rng.NextFloat() < QuestChance)
            {
                DoWhenChunkReady(Vector3.TransformPosition(Vector3.Zero, Rotation * Translation), P =>
                {
                    var position = Vector3.TransformPosition(Offset, Rotation) + P;
                    var npc = World.WorldBuilding.SpawnVillager(
                        position,
                        Rng
                    );
                    npc.Rotation = Physics.DirectionToEuler(npc.Orientation = -Vector3.TransformPosition(DefaultLookingDirection, Rotation));
                    npc.Physics.TargetPosition = position;
                    npc.Physics.UsePhysics = false;
                    npc.AddComponent(new QuestGiverComponent(npc, QuestPool.Grab().Build(npc.Position, Utils.Rng, npc)));
                    ((T) Structure.WorldObject).NPC = npc;
                }, Structure);
            }
        }
    }
}