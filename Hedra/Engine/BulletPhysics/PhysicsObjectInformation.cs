using Hedra.EntitySystem;

namespace Hedra.Engine.BulletPhysics
{
    public class PhysicsObjectInformation
    {
        public bool IsLand => !IsEntity;
        public bool IsEntity => Entity != null;
        public IEntity Entity { get; set; }
    }
}