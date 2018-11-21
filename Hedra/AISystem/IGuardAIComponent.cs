using Hedra.Engine.EntitySystem;
using OpenTK;

namespace Hedra.AISystem
{
    public interface IGuardAIComponent : IComponent<IEntity>
    {
        Vector3 GuardPosition { get; set; }
    }
}