using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem;

public class Cave4Cache : CacheType
{
    public Cave4Cache()
    {
        AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Caves/Cave4.ply", Vector3.One));

        AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Structures/Caves/Cave4.ply", Vector3.One));
    }

    public override CacheItem Type => CacheItem.Cave4;
    public static Vector3 Offset => Vector3.UnitY * 0.0f;

    public static Vector3 Lever0 => new Vector3(-71.14893f, 56.5f, 23.6824f);
}