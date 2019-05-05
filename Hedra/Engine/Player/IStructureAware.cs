using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.Player
{
    public interface IStructureAware
    {
        event OnStructureEnter StructureEnter;
        event OnStructureLeave StructureLeave;
        event OnStructureCompleted StructureCompleted;
        CollisionGroup[] NearCollisions { get; }
        void Update();
    }
}