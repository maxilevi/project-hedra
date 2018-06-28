﻿using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    internal class FernCache : CacheType
    {
        public FernCache()
        {
            this.AddModel(AssetManager.PlyLoader("Assets/Env/Fern.ply", Vector3.One));
        }
    }
}
