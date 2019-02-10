using Hedra.EntitySystem;

namespace Hedra.Components
{
    public class ExplorerThoughtsComponent : ThoughtsComponent
    {
        public ExplorerThoughtsComponent(IEntity Entity, params object[] Parameters) : base(Entity, Parameters)
        {
        }

        protected override string ThoughtKeyword => "explorer_thought";
    }
}