﻿using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class BossIconCache : CacheType
    {
        public BossIconCache()
        {
            this.AddModel(AssetManager.PlyLoader("Assets/Env/QuestIcon.ply", Vector3.One * 6f, -Vector3.UnitY, Vector3.Zero));
        }
    }
}