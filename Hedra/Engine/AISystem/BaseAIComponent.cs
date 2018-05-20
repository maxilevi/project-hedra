using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.AISystem
{
    public abstract class BaseAIComponent : EntityComponent
    {
        public bool Enabled { get; set; }

        protected BaseAIComponent(Entity Parent) : base(Parent)
        {

        }
    }
}
