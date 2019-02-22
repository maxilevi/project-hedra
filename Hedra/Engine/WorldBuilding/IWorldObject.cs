using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.WorldBuilding
{
    public delegate void OnDisposedEvent();
    
    public interface IWorldObject : IUpdatable
    {
        event OnDisposedEvent OnDispose;
        Vector3 Position { get; set; }
        void Dispose();
    }
}