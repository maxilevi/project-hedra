using System;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Mission;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class WizardTower : BaseStructure, ICompletableStructure
    {
        public bool Completed => throw new NotImplementedException();

        public WizardTower(Vector3 Position) : base(Position)
        {
        }
    }
}