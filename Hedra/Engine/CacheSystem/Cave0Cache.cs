using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem;

public class Cave0Cache : CacheType
{
    public Cave0Cache()
    {
        AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Caves/Cave0.ply", Vector3.One));

        AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Structures/Caves/Cave0.ply", Vector3.One));
    }

    public override CacheItem Type => CacheItem.Cave0;
    public static Vector3 Offset => Vector3.UnitY * 1.5f;
}