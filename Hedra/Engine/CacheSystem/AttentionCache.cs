using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    class AttentionCache : CacheType
    {
        public AttentionCache()
        {
            this.AddModel(AssetManager.PlyLoader("Assets/Env/ExclamationMark.ply", Vector3.One));
        }
    }
}