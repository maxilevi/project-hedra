using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;
using System.Numerics;

namespace Hedra.AISystem
{
    public interface IGuardAIComponent : IComponent<IEntity>
    {
        Vector3 GuardPosition { get; set; }
    }
}