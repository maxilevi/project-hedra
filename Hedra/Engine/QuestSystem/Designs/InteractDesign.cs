using Hedra.Engine.Player.QuestSystem.Views;

namespace Hedra.Engine.QuestSystem.Designs
{
    public class InteractDesign : PassthroughDesign
    {
        public override QuestTier Tier => QuestTier.Any;
        public override string Name => "InteractDesign";
        
        public override string GetShortDescription(QuestObject Quest)
        {
            throw new System.NotImplementedException();
        }

        public override string GetDescription(QuestObject Quest)
        {
            throw new System.NotImplementedException();
        }

        public override QuestView BuildView(QuestObject Quest)
        {
            throw new System.NotImplementedException();
        }

        protected override QuestDesign[] GetAuxiliaries(QuestObject Quest)
        {
            throw new System.NotImplementedException();
        }

        public override bool IsQuestCompleted(QuestObject Quest)
        {
            throw new System.NotImplementedException();
        }
    }
}