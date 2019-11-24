using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class Dungeon2IconCache : CacheType
    {
        public override CacheItem Type => CacheItem.Dungeon2Icon;

        public Dungeon2IconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon2-Icon.ply", Vector3.One));
        }
    }
}