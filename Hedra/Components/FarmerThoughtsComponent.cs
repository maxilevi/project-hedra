using Hedra.Engine.Localization;
using Hedra.EntitySystem;

namespace Hedra.Components
{
    public class FarmerThoughtsComponent : ThoughtsComponent
    {
        public override Translation[] Thoughts { get; } = new[]
        {
            Translation.Create("farmer_thought_1"),
            Translation.Create("farmer_thought_2"),
            Translation.Create("farmer_thought_3")
        };

        public FarmerThoughtsComponent(IEntity Entity) : base(Entity)
        {
        }
    }
}