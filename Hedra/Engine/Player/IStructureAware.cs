using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.Player
{
    public interface IStructureAware
    {
        event OnStructureEnter StructureEnter;
        CollisionGroup[] NearCollisions { get; }
        void Update();
    }
}