using Hedra.Engine.EntitySystem;
using Newtonsoft.Json.Serialization;

namespace Hedra.Engine.AISystem
{
    public abstract class BaseAIComponent : EntityComponent
    {
        public abstract AIType Type { get; }

        protected BaseAIComponent(Entity Parent) : base(Parent)
        {

        }
    }
}
