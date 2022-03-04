using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem;

public class Cave5Cache : CacheType
{
    public Cave5Cache()
    {
        AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Caves/Cave5.ply", Vector3.One));

        AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Structures/Caves/Cave5.ply", Vector3.One));
    }

    public override CacheItem Type => CacheItem.Cave5;
    public static Vector3 Offset => Vector3.UnitY * 0.5f;
}