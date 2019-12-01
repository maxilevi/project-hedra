using Hedra.Engine.Management;
using System.Numerics;

namespace Hedra.Engine.CacheSystem
{
    public class GazeboIconCache : CacheType
    {
        public override CacheItem Type => CacheItem.GazeboIcon;

        public GazeboIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Gazebo/Gazebo0-Icon.ply", Vector3.One));
        }
    }
}