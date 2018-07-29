﻿using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    [CacheIgnore]
    internal class FlowerCache : CacheType
    {
        public FlowerCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Tetrahedra.ply", Vector3.One));
        }
    }
}
