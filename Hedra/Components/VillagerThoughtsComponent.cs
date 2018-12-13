using Hedra.Engine.Localization;
using Hedra.EntitySystem;

namespace Hedra.Components
{
    public class VillagerThoughtsComponent : ThoughtsComponent
    {
        public VillagerThoughtsComponent(IEntity Entity) : base(Entity)
        {
        }

        protected override string ThoughtKeyword => "villager_thought";
    }
}