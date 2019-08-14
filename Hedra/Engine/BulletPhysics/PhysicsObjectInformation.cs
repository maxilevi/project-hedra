using BulletSharp;
using Hedra.Engine.Player;
using Hedra.EntitySystem;

namespace Hedra.Engine.BulletPhysics
{
    public class PhysicsObjectInformation
    {
        public string Name { get; set; }
        public CollisionFilterGroups Group { get; set; }
        public CollisionFilterGroups Mask { get; set; }
        public bool IsLand => !IsEntity;
        public bool IsEntity => Entity != null;
        public IEntity Entity { get; set; }
        public bool IsInSimulation { get; set; }
        public bool IsPlayer  => Entity is IPlayer;
    }
}