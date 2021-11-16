//#define DEBUG_MESH

using System.Numerics;
using BulletSharp;
using Hedra.Engine.Bullet;
using Hedra.Engine.WorldBuilding;
using Hedra.EntitySystem;
using Hedra.Rendering;

namespace Hedra.Engine.StructureSystem
{
    public delegate void OnCollisionEvent(IEntity Entity);

    public delegate void OnSeparationEvent(IEntity Entity);

    public class CollisionTrigger : BaseStructure
    {
        public const CollisionFilterGroups Group = CollisionFilterGroups.KinematicFilter;
        public event OnCollisionEvent OnCollision;
        public event OnSeparationEvent OnSeparation;

        private readonly RigidBody _sensor;
#if DEBUG_MESH
        private ObjectMesh _mesh;
#endif
        private Vector3 _position;

        public CollisionTrigger(Vector3 Position, VertexData Mesh) : base(Position)
        {
            Mesh = Mesh.AverageCenter();
#if DEBUG_MESH
            _mesh = ObjectMesh.FromVertexData(Mesh, false, true);
            _mesh.Enabled = true;
#endif
            var shape = BulletPhysics.CreateTriangleShape(Mesh.Indices, Mesh.Vertices);
            using (var bodyInfo = new RigidBodyConstructionInfo(0, new DefaultMotionState(), shape))
            {
                _sensor = new RigidBody(bodyInfo);
                _sensor.Translate(Position.Compatible());
                _sensor.CollisionFlags |= CollisionFlags.NoContactResponse;
                BulletPhysics.Add(_sensor, new PhysicsObjectInformation
                {
                    Group = Group,
                    /* If we add a sensor filter then we are colliding two times with the entities (one for body and another one for the sensor)*/
                    Mask = CollisionFilterGroups.CharacterFilter,
                    Name = $"Trigger at {Position}",
                    StaticOffsets = new[]
                    {
                        World.ToChunkSpace(Position)
                    }
                });
                _sensor.Gravity = BulletSharp.Math.Vector3.Zero;
            }

            BulletPhysics.OnCollision += OnWorldCollision;
            BulletPhysics.OnSeparation += OnWorldSeparation;
        }

        public override Vector3 Position
        {
            get => _position;
            set
            {
                _sensor?.Translate((-_position + value).Compatible());
#if DEBUG_MESH
                if(_mesh != null)
                    _mesh.Position = value;
#endif
                _position = value;
            }
        }

        private void OnWorldCollision(CollisionObject Object0, CollisionObject Object1)
        {
            if (!ProcessTrigger(Object0, Object1, out var entity)) return;
            OnCollision?.Invoke(entity);
        }

        private void OnWorldSeparation(CollisionObject Object0, CollisionObject Object1)
        {
            if (!ProcessTrigger(Object0, Object1, out var entity)) return;
            OnSeparation?.Invoke(entity);
        }

        private bool ProcessTrigger(CollisionObject Object0, CollisionObject Object1, out IEntity Against)
        {
#if DEBUG_MESH
            if(_mesh != null)
                _mesh.Position = _position;
#endif
            Against = null;
            if (!ReferenceEquals(Object0, _sensor) && !ReferenceEquals(Object1, _sensor)) return false;
            var other = ReferenceEquals(Object0, _sensor) ? Object1 : Object0;
            var information = (PhysicsObjectInformation)other.UserObject;
            if (!information.IsEntity) return false;
            Against = information.Entity;
            return true;
        }

        public override void Dispose()
        {
            BulletPhysics.RemoveAndDispose(_sensor);
        }
    }
}