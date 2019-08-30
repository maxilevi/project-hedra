using System.Collections.Generic;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    public class FishingPostCache : CacheType
    {
        public override CacheItem Type => CacheItem.FishingPost;

        public FishingPostCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/FishingSettlement/FishingDock0-Mesh.ply", Scale));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Structures/FishingSettlement/FishingDock0.ply", Scale));
        }
        
        public static Vector3 Scale => Vector3.One * 11f;
    }
}