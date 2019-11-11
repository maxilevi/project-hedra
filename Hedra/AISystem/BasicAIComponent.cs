using Hedra.EntitySystem;

namespace Hedra.AISystem
{
    public abstract class BasicAIComponent : GenericBasicAIComponent<IEntity>, IBehaviourComponent, ITraverseAIComponent
    {
        protected BasicAIComponent(IEntity Parent) : base(Parent)
        {
        }
    }
}
