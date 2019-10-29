using Hedra.EntitySystem;

namespace Hedra.Components
{
    public class DarkWizardThoughtsComponent : ThoughtsComponent
    {
        public DarkWizardThoughtsComponent(IEntity Entity, params object[] Parameters) : base(Entity, Parameters)
        {
        }

        protected override string ThoughtKeyword => "dark_wizard_thought";
    }
}