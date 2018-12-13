using Hedra.EntitySystem;

namespace Hedra.Components
{
    public class SpawnVillagerThoughtsComponent : ThoughtsComponent
    {
        public SpawnVillagerThoughtsComponent(IEntity Entity) : base(Entity)
        {
        }

        protected override string ThoughtKeyword => "spawn_dialog";
    }
}