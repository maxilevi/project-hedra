using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class GarrisonCache : CacheType
    {
        public override CacheItem Type => CacheItem.Garrison;

        public GarrisonCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Garrison/Garrison0-Mesh.ply", Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Structures/Garrison/Garrison0.ply", Vector3.One));
        }

        public static Vector3[] Doors { get; } = new Vector3[4]
        {
            new Vector3(-69.41303f, 7.28182f, 20.79085f),
            new Vector3(-23.36839f, 8.57924f, -11.57599f),
            new Vector3(67.29353f, 7.25386f, -49.92051f),
            new Vector3(26.6138f, 38.44756f, -15.21347f)
        };
        
        public static Vector3 Scale => Vector3.One * 1.1f;
    }
}