using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class GnollFortressIconCache : CacheType
    {
        public override CacheItem Type => CacheItem.GnollFortressIcon;

        public GnollFortressIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/GnollFortress/GnollFortress0-Icon.ply", Vector3.One));
        }
    }
}