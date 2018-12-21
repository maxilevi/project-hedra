using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.Player
{
    public interface IStructureAware
    {
        CollisionGroup[] NearCollisions { get; }
        void Update();
    }
}