using Hedra.EntitySystem;

namespace Hedra.Components
{
    public class WizardThoughtsComponent : ThoughtsComponent
    {
        public WizardThoughtsComponent(IEntity Entity, params object[] Parameters) : base(Entity, Parameters)
        {
        }

        protected override string ThoughtKeyword => "wizard_thought";
    }
}