using Hedra.Engine.Management;
using System.Numerics;

namespace Hedra.Engine.CacheSystem
{
    public class GazeboCache : CacheType
    {
        public override CacheItem Type => CacheItem.Gazebo;

        public GazeboCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Gazebo0.ply", Scale));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Structures/Gazebo0.ply", Scale));
        }
        
        private static Vector3 Scale => Vector3.One * 12f;
        public static Vector3 OffsetFromGround => Vector3.UnitY * .45f * Scale;
    }
}