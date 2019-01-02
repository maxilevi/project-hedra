using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.Player.QuestSystem.Views
{
    public class EntityView : QuestView
    {
        private readonly AnimatedUpdatableModel _model;
        
        public EntityView(QuestObject Quest, AnimatedUpdatableModel Model) : base(Quest)
        {
            _model = Model;
        }
        
        public override uint GetTextureId()
        {
            return _model.DrawPreview();
        }
    }
}