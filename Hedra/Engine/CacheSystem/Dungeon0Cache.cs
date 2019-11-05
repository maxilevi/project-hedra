using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class Dungeon0Cache : CacheType
    {
        public override CacheItem Type => CacheItem.Dungeon0;

        public Dungeon0Cache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon0.ply", Vector3.One));
            
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Structures/Dungeon/Dungeon0.ply", Vector3.One));
        }
    }
}