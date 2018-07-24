﻿using OpenTK;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    internal class GiantTreeCache : CacheType
    {
        public GiantTreeCache()
        {
            this.AddModel(AssetManager.PlyLoader("Assets/Env/GiantTree0.ply", Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/GiantTree0.ply", 77, Vector3.One));
        }
    }
}