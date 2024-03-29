using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class GraveCache : CacheType
    {
        public GraveCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Grave0.ply", Vector3.One));
            AddModel(AssetManager.PLYLoader("Assets/Env/Grave1.ply", Vector3.One));
            AddModel(AssetManager.PLYLoader("Assets/Env/Grave2.ply", Vector3.One));
            AddModel(AssetManager.PLYLoader("Assets/Env/Grave3.ply", Vector3.One));
            AddModel(AssetManager.PLYLoader("Assets/Env/Grave4.ply", Vector3.One));
            AddModel(AssetManager.PLYLoader("Assets/Env/Grave5.ply", Vector3.One));

            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Grave0.ply", 1, Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Grave1.ply", 1, Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Grave2.ply", 1, Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Grave3.ply", 1, Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Grave4.ply", 1, Vector3.One));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Grave5.ply", 1, Vector3.One));
        }

        public override CacheItem Type => CacheItem.Grave;
    }
}