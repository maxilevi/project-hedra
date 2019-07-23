using Hedra.Engine.QuestSystem;

namespace Hedra.Engine.Player.QuestSystem.Views
{
    public abstract class QuestView
    {
        public abstract uint GetTextureId();

        public virtual void Dispose()
        {
        }
    }
}