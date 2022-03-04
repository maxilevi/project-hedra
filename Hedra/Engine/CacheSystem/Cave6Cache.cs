using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem;

public class Cave6Cache : CacheType
{
    public Cave6Cache()
    {
        AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Caves/Cave6.ply", Vector3.One));

        AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Structures/Caves/Cave6.ply", Vector3.One));
    }

    public override CacheItem Type => CacheItem.Cave6;
    public static Vector3 Offset => Vector3.UnitY * 0.5f;
}