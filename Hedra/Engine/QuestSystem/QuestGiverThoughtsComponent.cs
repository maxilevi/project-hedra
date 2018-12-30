using Hedra.Components;
using Hedra.EntitySystem;

namespace Hedra.Engine.QuestSystem
{
    public class QuestGiverThoughtsComponent : ThoughtsComponent
    {
        public QuestGiverThoughtsComponent(IEntity Entity) : base(Entity)
        {
        }

        protected override string ThoughtKeyword => "quest_giver_thought";
    }
}