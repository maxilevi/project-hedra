using Hedra.Engine.Management;
using OpenTK;

namespace Hedra.Engine.CacheSystem
{
    internal class OakTreesCache : CacheType
    {
        public OakTreesCache()
        {
            this.AddModel(AssetManager.PlyLoader("Assets/Env/Oak0.ply", Vector3.One));
            this.AddModel(AssetManager.PlyLoader("Assets/Env/Oak1.ply", Vector3.One));
            this.AddModel(AssetManager.PlyLoader("Assets/Env/Oak2.ply", Vector3.One));

            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Oak0.ply", 10, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Oak1.ply", 2, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Oak2.ply", 6, Vector3.One));
        }
    }
}
