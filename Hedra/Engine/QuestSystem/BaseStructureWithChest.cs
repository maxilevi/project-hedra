using Hedra.Engine.ItemSystem;
using Hedra.Engine.Localization;
using Hedra.Engine.WorldBuilding;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.QuestSystem
{
    public abstract class BaseStructureWithChest : BaseStructure
    {
        public Chest Chest { get; set; }
        
        protected BaseStructureWithChest(Vector3 Position) : base(Position)
        {
        }
    }
}