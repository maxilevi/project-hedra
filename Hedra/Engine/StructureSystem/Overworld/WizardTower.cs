using System;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.WorldBuilding;
using OpenTK;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class WizardTower : BaseStructure, ICompletableStructure
    {
        public bool Completed => throw new NotImplementedException();
        public ItemDescription DeliveryItem => throw new NotImplementedException();

        public WizardTower(Vector3 Position) : base(Position)
        {
        }
        public QuestReward Reward => null;
    }
}