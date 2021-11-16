using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class WoodLogCache : CacheType
    {
        public WoodLogCache()
        {
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/WoodLog0.ply", Vector3.One));
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/WoodLog1.ply", Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Plants/WoodLog0.ply", Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Plants/WoodLog0.ply", Vector3.One));
        }

        public override CacheItem Type => CacheItem.WoodLog;
    }
}