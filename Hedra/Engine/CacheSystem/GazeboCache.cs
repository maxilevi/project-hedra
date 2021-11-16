using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class GazeboCache : CacheType
    {
        public GazeboCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Gazebo/Gazebo0.ply", Scale));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Structures/Gazebo/Gazebo0.ply", Scale));
        }

        public override CacheItem Type => CacheItem.Gazebo;

        private static Vector3 Scale => Vector3.One * 12f;
        public static Vector3 OffsetFromGround => Vector3.UnitY * .45f * Scale;
    }
}