using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class Dungeon0 : BaseStructure, ICompletableStructure
    {
        public IEntity Boss { get; set; }
        public Dungeon0Trigger Trigger { get; set; }

        public Dungeon0(Vector3 Position) : base(Position)
        {
        }

        public void Reset()
        {
            Trigger.Reset();
        }

        public bool Completed => Boss.IsDead;
    }
}