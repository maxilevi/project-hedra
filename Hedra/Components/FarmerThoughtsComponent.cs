using Hedra.EntitySystem;

namespace Hedra.Components
{
    public class FarmerThoughtsComponent : ThoughtsComponent
    {
        public FarmerThoughtsComponent(IEntity Entity) : base(Entity)
        {
        }

        protected override string ThoughtKeyword => "farmer_thought";
    }
}