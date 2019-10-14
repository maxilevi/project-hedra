using System;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using System.Numerics;

namespace Hedra.Engine.CacheSystem
{
    public class CampfireIconCache : CacheType
    {
        public CampfireIconCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/UI/CampfireIcon.ply", Vector3.One * .75f));
        }
        
        public override CacheItem Type => CacheItem.CampfireIcon;
    }
}
