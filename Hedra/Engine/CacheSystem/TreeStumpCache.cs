using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class TreeStumpCache : CacheType
    {
        public override CacheItem Type => CacheItem.TreeStump;

        public TreeStumpCache()
        {
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/TreeStump0.ply", Vector3.One));
            AddModel(AssetManager.LoadPLYWithLODs("Assets/Env/Plants/TreeStump1.ply", Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Plants/TreeStump0.ply", Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Plants/TreeStump0.ply", Vector3.One));
        }
    }
}