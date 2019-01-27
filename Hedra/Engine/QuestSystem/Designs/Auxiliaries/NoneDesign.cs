using Hedra.Engine.Player.QuestSystem.Views;

namespace Hedra.Engine.QuestSystem.Designs.Auxiliaries
{
    public class NoneDesign : AuxiliaryDesign
    {
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

        public override bool IsQuestCompleted(QuestObject Quest) => true;
    }
}