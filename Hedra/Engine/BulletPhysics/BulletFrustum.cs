using System.Collections.Generic;
using BulletSharp;
using Hedra.Engine.Management;

namespace Hedra.Engine.BulletPhysics
{
    public class BulletFrustum
    {
        private Dictionary<ICullable, RigidBody> _bodies;
        private DiscreteDynamicsWorld _world;

        public BulletFrustum()
        {
            _bodies = new Dictionary<ICullable, RigidBody>();
            var configuration = new DefaultCollisionConfiguration();
            _world = new DiscreteDynamicsWorld(new CollisionDispatcher(configuration), new DbvtBroadphase(), new SequentialImpulseConstraintSolver(), configuration);
        }

        public void Add(ICullable Cullable)
        {
            _bodies.Add(Cullable, CreateRigidbody(Cullable));
            _world.AddRigidBody(_bodies[Cullable]);
        }

        public void Remove(ICullable Cullable)
        {
            var body = _bodies[Cullable];
            _world.RemoveRigidBody(body);
            _bodies.Remove(Cullable);
            DestroyRigidbody(body);
        }
        private static RigidBody CreateRigidbody(ICullable Cullable)
        {
            var shape = new BoxShape((Cullable.Max - Cullable.Min).Compatible() * .5f);
            using (var bodyInfo = new RigidBodyConstructionInfo(0, new DefaultMotionState(), shape))
            {
                var rigid = new RigidBody(bodyInfo);
                rigid.CollisionFlags |= CollisionFlags.NoContactResponse;
                return rigid;
            }
        }

        private static void DestroyRigidbody(RigidBody Body)
        {
            Body.CollisionShape.Dispose();
            Body.MotionState.Dispose();
            Body.Dispose();
        }
    }
}