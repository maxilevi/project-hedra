using System;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using Hedra.Mission;
using System.Numerics;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class WizardTower : BaseStructure, ICompletableStructure
    {
        public bool Completed => false;

        public WizardTower(Vector3 Position) : base(Position)
        {
        }
    }
}