using System;
using System.Linq;
using System.Numerics;
using Hedra.Engine.Generation;
using Hedra.Engine.Scenes;

namespace Hedra.Engine.StructureSystem.Overworld
{
    public abstract class DungeonWithBossDesign : BaseDungeonDesign<DungeonWithBoss>
    {
        protected override DungeonWithBoss Create(Vector3 Position, float Size)
        {
            return new DungeonWithBoss(Position);
        }
        
        protected override void DoBuild(CollidableStructure Structure, Matrix4x4 Rotation, Matrix4x4 Translation, Random Rng)
        {
            base.DoBuild(Structure, Rotation, Translation, Rng);
            SceneLoader.LoadIfExists(Structure, $"Assets/Env/Structures/Dungeon/{BaseFileName}.ply", Vector3.One, Rotation * Translation, Scene);
            ((DungeonWithBoss)Structure.WorldObject).BuildingTrigger = (DungeonDoorTrigger) Structure.WorldObject.Children.FirstOrDefault(T => T is DungeonDoorTrigger);
            Structure.Waypoints = WaypointLoader.Load($"Assets/Env/Structures/Dungeon/{BaseFileName}-Pathfinding.ply", Vector3.One, Rotation * Translation);
        }
        
        protected abstract SceneSettings Scene { get; }
        
        protected abstract string BaseFileName { get; }
    }
}