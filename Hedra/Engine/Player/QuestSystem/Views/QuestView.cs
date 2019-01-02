using Hedra.Engine.QuestSystem;

namespace Hedra.Engine.Player.QuestSystem.Views
{
    public abstract class QuestView
    {
        protected QuestObject Quest { get; }

        protected QuestView(QuestObject Quest)
        {
            this.Quest = Quest;
        }
        
        public abstract uint GetTextureId();
    }
}