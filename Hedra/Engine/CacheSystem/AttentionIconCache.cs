using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    internal class AttentionIconCache : CacheType
    {
        public AttentionIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/ExclamationMark.ply", Vector3.One));
        }

        public override CacheItem Type => CacheItem.AttentionIcon;
    }
}