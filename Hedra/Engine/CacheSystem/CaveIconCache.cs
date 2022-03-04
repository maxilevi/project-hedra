using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem;

public class CaveIconCache : CacheType
{
    public CaveIconCache()
    {
        AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Caves/CaveIcon.ply", Vector3.One));
    }

    public override CacheItem Type => CacheItem.CaveIcon;
}

