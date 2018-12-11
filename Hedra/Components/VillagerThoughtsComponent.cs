using Hedra.Engine.Localization;
using Hedra.EntitySystem;

namespace Hedra.Components
{
    public class VillagerThoughtsComponent : ThoughtsComponent
    {
        public VillagerThoughtsComponent(IEntity Entity) : base(Entity)
        {
        }

        public override Translation[] Thoughts { get; } = new[]
        {
            Translation.Create("villager_thought_1"),
            Translation.Create("villager_thought_2"),
            Translation.Create("villager_thought_3"),
            Translation.Create("villager_thought_4")
        };
    }
}