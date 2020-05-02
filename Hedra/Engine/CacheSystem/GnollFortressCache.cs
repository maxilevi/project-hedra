using System.Numerics;
using Hedra.Engine.Management;
using Hedra.Engine.StructureSystem;

namespace Hedra.Engine.CacheSystem
{
    public class GnollFortressCache : CacheType
    {
        public override CacheItem Type => CacheItem.GnollFortress;

        public GnollFortressCache()
        {
            AddModel(AssetManager.PLYLoader("Assets/Env/Structures/GnollFortress/GnollFortress0.ply", Scale));
            AddShapes(AssetManager.LoadCollisionShapes("Assets/Env/Structures/GnollFortress/GnollFortress0.ply", Scale));
        }
        
        public static Vector3 Scale => Vector3.One * 8f;

        public static DoorSettings[] DoorSettings { get; } = new DoorSettings[11]
        {
            new DoorSettings(new Vector3(4.15415f, 0.68127f, -25.87071f), Scale, true, true),
            new DoorSettings(new Vector3(-3.67286f, 0.67368f, -25.90326f), Scale, true, false),
            new DoorSettings(new Vector3(8.9148f, 0.67694f, -5.10739f), Scale, true, true),
            new DoorSettings(new Vector3(14.32418f, 0.67368f, -20.53465f), Scale, false, false),
            new DoorSettings(new Vector3(0.33746f, 0.58755f, -4.30255f), Scale, true, true),
            new DoorSettings(new Vector3(-5.54441f, 0.68167f, -10.79792f), Scale, true, false),
            new DoorSettings(new Vector3(-0.2737f, 0.58719f, -4.29512f), Scale, false, false),
            new DoorSettings(new Vector3(-5.77464f, 0.72194f, 12.99786f), Scale, true, false),
            new DoorSettings(new Vector3(-8.38686f, 0.67694f, -5.3287f), Scale, true, false),
            new DoorSettings(new Vector3(4.54559f, 0.72194f, 13.13675f), Scale, true, true),
            new DoorSettings(new Vector3(5.24249f, 0.67368f, -10.24741f), Scale, false, true)
        };

        public static string PathfindingFile => $"Assets/Env/Structures/GnollFortress/GnollFortress0-Pathfinding.ply";
        public static string SceneFile => $"Assets/Env/Structures/GnollFortress/GnollFortress0.ply";
    }
}