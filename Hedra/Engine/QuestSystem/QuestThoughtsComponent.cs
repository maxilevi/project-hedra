using Hedra.Components;
using Hedra.EntitySystem;

namespace Hedra.Engine.QuestSystem
{
    public class QuestThoughtsComponent : ThoughtsComponent
    {
        public QuestThoughtsComponent(IEntity Entity, string Keyword, params object[] Parameters) : base(Entity, Parameters)
        {
            ThoughtKeyword = Keyword;
            UpdateThoughts();
        }

        protected override string ThoughtKeyword { get; }
    }
}