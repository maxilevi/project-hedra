using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class Dungeon0IconCache : CacheType
    {
        public override CacheItem Type => CacheItem.Dungeon0Icon;

        public Dungeon0IconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon0-Icon.ply", Vector3.One));
        }
    }
}