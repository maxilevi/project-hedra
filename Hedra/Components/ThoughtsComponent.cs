using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.EntitySystem;

namespace Hedra.Components
{
    public abstract class ThoughtsComponent : EntityComponent
    {
        public abstract Translation[] Thoughts { get; }

        protected ThoughtsComponent(IEntity Entity) : base(Entity)
        {
        }

        public override void Update()
        {
        }
    }
}