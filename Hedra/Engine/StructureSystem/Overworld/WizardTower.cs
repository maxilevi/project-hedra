using System.Numerics;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class WizardTower : BaseStructure, ICompletableStructure
    {
        public WizardTower(Vector3 Position) : base(Position)
        {
        }

        public bool Completed => false;
    }
}