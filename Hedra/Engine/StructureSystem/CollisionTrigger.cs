using System.Numerics;
using BulletSharp;
using Hedra.Engine.Bullet;
using Hedra.Engine.WorldBuilding;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem
{
    public class CollisionTrigger : BaseStructure
    {
        private RigidBody _sensor;
        
        public CollisionTrigger(Vector3 Position, VertexData Model) : base(Position)
        {
            using (var bodyInfo = new RigidBodyConstructionInfo(0, new DefaultMotionState(), BuildShape(Mesh)))
            {
                _sensor = new RigidBody(bodyInfo);
                _sensor.Translate(Position.Compatible());
                Bullet.BulletPhysics.Add(_sensor, new PhysicsObjectInformation
                {
                    Group = CollisionFilterGroups.SensorTrigger,
                    Mask = (CollisionFilterGroups.AllFilter & ~CollisionFilterGroups.SensorTrigger),
                    Name = $"Trigger at {Position}",
                    StaticOffsets = new []{World.ToChunkSpace(Position)}
                });
                _sensor.Gravity = BulletSharp.Math.Vector3.Zero;
            }
            
            BulletPhysics.OnCollision += OnCollision;
            BulletPhysics.OnSeparation += OnSeparation;
        }

        private void OnCollision(CollisionObject Object0, CollisionObject Object1)
        {
            if (!ReferenceEquals(Object0, _sensor) && !ReferenceEquals(Object1, _sensor)) return;
            var other = ReferenceEquals(Object0, _sensor) ? Object1 : Object0;
            if (!ReferenceEquals(other, _body))
            {
                _sensorContacts++;
            }
        }
        
        private void OnSeparation(CollisionObject Object0, CollisionObject Object1)
        {
            if (!ReferenceEquals(Object0, _sensor) && !ReferenceEquals(Object1, _sensor)) return;
            var other = ReferenceEquals(Object0, _sensor) ? Object1 : Object0;
            if (!ReferenceEquals(other, _body))
            {
                _sensorContacts--;
            }
        }
        
        public void Dispose()
        {
            BulletPhysics.RemoveAndDispose(_sensor);
        }
    }
}