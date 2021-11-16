using System.Numerics;
using Hedra.EntitySystem;

namespace Hedra.AISystem
{
    public interface IGuardAIComponent : IComponent<IEntity>
    {
        Vector3 GuardPosition { get; set; }
    }
}