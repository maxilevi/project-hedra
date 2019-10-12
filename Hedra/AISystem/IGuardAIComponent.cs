using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;
using OpenToolkit.Mathematics;

namespace Hedra.AISystem
{
    public interface IGuardAIComponent : IComponent<IEntity>
    {
        Vector3 GuardPosition { get; set; }
    }
}