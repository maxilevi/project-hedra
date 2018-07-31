using OpenTK;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class GraveCache : CacheType
    {
        public GraveCache()
        {
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Grave0.ply", Vector3.One));
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Grave1.ply", Vector3.One));
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Grave2.ply", Vector3.One));
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Grave3.ply", Vector3.One));
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Grave4.ply", Vector3.One));
            this.AddModel(AssetManager.PLYLoader("Assets/Env/Grave5.ply", Vector3.One));
            
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Grave0.ply", 1, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Grave1.ply", 1, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Grave2.ply", 1, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Grave3.ply", 1, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Grave4.ply", 1, Vector3.One));
            this.AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Grave5.ply", 1, Vector3.One));
        }
    }
}