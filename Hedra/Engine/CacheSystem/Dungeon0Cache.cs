using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class Dungeon0Cache : CacheType
    {
        public override CacheItem Type => CacheItem.Dungeon0;

        public Dungeon0Cache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon0.ply", Vector3.One));
            
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Structures/Dungeon/Dungeon0.ply", Vector3.One));
        }
        
        public static Vector3[] Doors { get; } = new Vector3[7]
        {
            new Vector3(-30.96755f, 15.9269f, 32.96857f),
            new Vector3(-68.90638f, 15.9269f, -10.92082f),
            new Vector3(-30.41704f, 15.92689f, -51.85977f),
            new Vector3(20.72186f, 15.92689f, -61.47021f),
            new Vector3(10.72188f, 15.9269f, -9.17032f),
            new Vector3(47.17975f, 15.92689f, -0.73141f),
            new Vector3(59.23969f, 15.92689f, -0.73141f)
        };

        public static Vector3 Offset { get; } = Vector3.UnitY * -.3f;
    }
}