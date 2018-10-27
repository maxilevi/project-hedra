using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.Player
{
    public interface IStructureAware
    {
        ICollidable[] NearCollisions { get; }
        void Update();
    }
}