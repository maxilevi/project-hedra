using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.QuestSystem
{
    public class QuestStructure : BaseStructure
    {
        private readonly IHumanoid _giver;
        
        public QuestStructure(Vector3 Position, IHumanoid Giver) : base(Position)
        {
            _giver = Giver;
        }

        public override void Dispose()
        {
            QuestPersistence.UnregisterNPC(_giver);
            base.Dispose();
        }
    }
}