using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class GazeboIconCache : CacheType
    {
        public GazeboIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Gazebo/Gazebo0-Icon.ply", Vector3.One));
        }

        public override CacheItem Type => CacheItem.GazeboIcon;
    }
}