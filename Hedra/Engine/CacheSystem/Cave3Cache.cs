using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem;

public class Cave3Cache : CacheType
{
    public Cave3Cache()
    {
        AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Caves/Cave3.ply", Vector3.One));

        AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Structures/Caves/Cave3.ply", Vector3.One));
    }

    public override CacheItem Type => CacheItem.Cave3;
    public static Vector3 Offset => Vector3.UnitY * 0.5f;
}