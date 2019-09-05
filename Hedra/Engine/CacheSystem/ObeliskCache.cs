using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class ObeliskCache : CacheType
    {
        public ObeliskCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Obelisk/Obelisk0.ply", Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Structures/Obelisk/Obelisk0.ply", Vector3.One));
        }
        
        public override CacheItem Type => CacheItem.Obelisk;
    }
}