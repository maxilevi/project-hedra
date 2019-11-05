using System.Numerics;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class Dungeon : BaseStructure, ICompletableStructure
    {
        public Dungeon(Vector3 Position) : base(Position)
        {
        }

        public bool Completed { get; }
    }
}