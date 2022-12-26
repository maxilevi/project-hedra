using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem;

public class MushroomGrassCache : CacheType
{
    public MushroomGrassCache()
    {
        AddModel(AssetManager.PLYLoader("Assets/Env/Plants/MushroomGrass0.ply", Vector3.One));
        AddModel(AssetManager.PLYLoader("Assets/Env/Plants/MushroomGrass1.ply", Vector3.One * 0.5f));
    }

    public override CacheItem Type => CacheItem.MushroomGrass;
}