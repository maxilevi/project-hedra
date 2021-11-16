using System.Numerics;
using Hedra.Engine.WorldBuilding;

namespace Hedra.Engine.QuestSystem
{
    public abstract class BaseStructureWithChest : BaseStructure
    {
        protected BaseStructureWithChest(Vector3 Position) : base(Position)
        {
        }

        public Chest Chest { get; set; }
    }
}