using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class WitchHutIcon : CacheType
    {
        public override CacheItem Type => CacheItem.WitchHutIcon;
        
        public WitchHutIcon()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/WitchHut0-Icon.ply", Vector3.One));
        }
    }
}