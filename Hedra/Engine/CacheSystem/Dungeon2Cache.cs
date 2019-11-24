using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class Dungeon2Cache : CacheType
    {
        public override CacheItem Type => CacheItem.Dungeon2;
        
        public Dungeon2Cache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon2.ply", Vector3.One));
            
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Structures/Dungeon/Dungeon2.ply", Vector3.One));
        }
        
        public static Vector3[] Doors { get; } = new Vector3[7]
        {
            new Vector3(23.72799f, 20.9819f, -26f),
            new Vector3(9.38795f, 21.22655f, 38.62936f),
            new Vector3(-5.89023f, 20.9819f, -26f),
            new Vector3(-100f, 20.9819f, -10.50791f),
            new Vector3(115.0f, 20.9819f, -10.00166f),
            new Vector3(65.03032f, 20.9819f, -83.45332f),
            new Vector3(-47.70391f, 20.9819f, -83.75f),
        };
        
        public static Vector3 Lever0 => new Vector3(-87.67147f, 12.65296f, -45.62317f);

        public static Vector3 Scale { get; } = Vector3.One;
        
        public static Vector3 Offset { get; } = Vector3.UnitY * .25f;
    }
}