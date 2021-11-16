using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class Dungeon1IconCache : CacheType
    {
        public Dungeon1IconCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon1-Icon.ply", Vector3.One));
        }

        public override CacheItem Type => CacheItem.Dungeon1Icon;
    }
}