using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class VillageIconCache : CacheType
    {
        public VillageIconCache()
        {
            this.AddModel(
                AssetManager.PLYLoader("Assets/Env/Village/House0-Lod.ply", Vector3.One * .35f) 
                + AssetManager.PLYLoader("Assets/Env/Village/House0_Door0.ply", Vector3.One * .35f)
            );
        }
        
        public override CacheItem Type => CacheItem.VillageIcon;
    }
}
