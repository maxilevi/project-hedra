using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.EntitySystem;
using System.Numerics;

namespace Hedra.Engine.WorldBuilding
{
    public delegate void OnDisposedEvent();
    
    public interface IWorldObject : IUpdatable
    {
        Vector3 Position { get; set; }
        void Dispose();
    }
}