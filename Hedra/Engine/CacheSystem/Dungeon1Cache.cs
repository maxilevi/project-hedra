using System.Numerics;
using Hedra.Engine.Management;

namespace Hedra.Engine.CacheSystem
{
    public class Dungeon1Cache : CacheType
    {
        public override CacheItem Type => CacheItem.Dungeon1;

        public Dungeon1Cache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/Dungeon/Dungeon1.ply", Vector3.One));
            
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Structures/Dungeon/Dungeon1.ply", Vector3.One));
        }
        
        public static Vector3[] Doors { get; } = new Vector3[7]
        {
            new Vector3(-1.11255f, 13.51773f, -131.26634f),
            new Vector3(20.59462f, 12.58663f, -102.74712f),
            new Vector3(101.19066f, 12.58664f, 33.09047f),
            new Vector3(-17.7512f, 12.58663f, -102.74712f),
            new Vector3(-98.2591f, 12.58663f, 3.62671f),
            new Vector3(-7.49554f, 12.70829f, 3.41794f),
            new Vector3(7.35316f, 12.70829f, 3.41794f)
        };
        
        public static Vector3 Lever0 => new Vector3(77.18913f, 4.57114f, 216.17668f);

        public static Vector3 Scale { get; } = Vector3.One;
        
        public static Vector3 Offset { get; } = Vector3.UnitY * 9f;
    }
}