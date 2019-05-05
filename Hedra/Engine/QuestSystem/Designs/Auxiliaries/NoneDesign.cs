using Hedra.Engine.Player.QuestSystem.Views;

namespace Hedra.Engine.QuestSystem.Designs.Auxiliaries
{
    public class NoneDesign : AuxiliaryDesign
    {
        public override bool ShowPlaque => false;

        public override string GetShortDescription(QuestObject Quest)
        {
            return string.Empty;
        }

        public override string GetDescription(QuestObject Quest)
        {
            return string.Empty;
        }

        public override QuestView BuildView(QuestObject Quest)
        {
            return default(QuestView);
        }

        public override bool IsQuestCompleted(QuestObject Quest) => true;
    }
}