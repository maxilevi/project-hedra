using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class DungeonIconCache : CacheType
    {
        public override CacheItem Type => CacheItem.Dungeon0Icon;

        public DungeonIconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon0-Icon.ply", Vector3.One));
        }
    }
}