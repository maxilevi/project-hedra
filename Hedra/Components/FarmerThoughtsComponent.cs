using Hedra.Engine.Localization;
using Hedra.EntitySystem;

namespace Hedra.Components
{
    public class FarmerThoughtsComponent : ThoughtsComponent
    {
        protected override string ThoughtKeyword => "farmer_thought";

        public FarmerThoughtsComponent(IEntity Entity) : base(Entity)
        {
        }
    }
}