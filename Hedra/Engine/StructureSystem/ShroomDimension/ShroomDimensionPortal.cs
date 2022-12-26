using System.Numerics;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public class ShroomDimensionPortal : Portal
    {
        public ShroomDimensionPortal(Vector3 Position, Vector3 Scale, int Realm, Vector3 DefaultSpawn) : base(Position,
            Scale, Realm, DefaultSpawn)
        {
        }
        
        public ShroomDimensionPortal(Vector3 Position, Vector3 Scale, int Realm) : base(Position,
            Scale, Realm)
        {
        }
    }
}