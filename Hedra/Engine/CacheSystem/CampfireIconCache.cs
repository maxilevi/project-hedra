using System;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    internal class CampfireIconCache : CacheType
    {
        public CampfireIconCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/UI/CampfireIcon.ply", Vector3.One * .75f));
        }
    }
}
