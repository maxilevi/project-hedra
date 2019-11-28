using Hedra.Components;
using Hedra.EntitySystem;

namespace Hedra.Engine.QuestSystem
{
    public class ForgotQuestThoughtsComponent : ThoughtsComponent
    {
        public ForgotQuestThoughtsComponent(IEntity Entity, params object[] Parameters) : base(Entity, Parameters)
        {
        }

        protected override string ThoughtKeyword => "quest_failed_to_create";
    }
}