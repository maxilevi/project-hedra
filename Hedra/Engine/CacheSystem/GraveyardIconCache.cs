﻿using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class GraveyardIconCache : CacheType
    {
        public GraveyardIconCache()
        {
            this.AddModel(AssetManager.PlyLoader("Assets/Env/Grave4.ply", Vector3.One * 2f));
        }
    }
}