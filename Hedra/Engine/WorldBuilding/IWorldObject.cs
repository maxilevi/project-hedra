using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.WorldBuilding
{
    public delegate void OnDisposedEvent();

    public interface IWorldObject : IUpdatable
    {
        Vector3 Position { get; set; }
        void Dispose();
    }
}