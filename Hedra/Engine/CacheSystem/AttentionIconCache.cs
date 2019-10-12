using Hedra.Engine.Management;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.CacheSystem
{
    class AttentionIconCache : CacheType
    {
        public AttentionIconCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/ExclamationMark.ply", Vector3.One));
        }
        
        public override CacheItem Type => CacheItem.AttentionIcon;
    }
}