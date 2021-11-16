using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.Player
{
    public interface IStructureAware
    {
        CollisionGroup[] NearCollisions { get; }
        event OnStructureEnter StructureEnter;
        event OnStructureLeave StructureLeave;
        event OnStructureCompleted StructureCompleted;
        void Discard();
        void Update();
    }
}