using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    class AttentionIconCache : CacheType
    {
        public AttentionIconCache()
        {
            this.AddModel(AssetManager.PlyLoader("Assets/Env/ExclamationMark.ply", Vector3.One));
        }
    }
}