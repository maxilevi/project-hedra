using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class Dungeon0IconCache : CacheType
    {
        public Dungeon0IconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon0-Icon.ply", Vector3.One));
        }

        public override CacheItem Type => CacheItem.Dungeon0Icon;
    }
}