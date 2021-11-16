using System;
using BulletSharp;
using BulletSharp.Math;

namespace Hedra.Engine.Bullet
{
    public class BulletWorld : IDisposable
    {
        private readonly DefaultCollisionConfiguration _configuration;
        private readonly SequentialImpulseConstraintSolver _solver;

        public BulletWorld(float Gravity, bool ForceUpdateAllAabbs = true)
        {
            _configuration = new DefaultCollisionConfiguration();
            Dispatcher = new CollisionDispatcher(_configuration);
            Broadphase = new DbvtBroadphase();
            _solver = new SequentialImpulseConstraintSolver();
            DynamicsWorld = new DiscreteDynamicsWorld(Dispatcher, Broadphase, _solver, _configuration)
            {
                DebugDrawer = new BulletDraw
                {
                    DebugMode = DebugDrawModes.DrawWireframe
                },
                Gravity = -Vector3.UnitY * Gravity,
                ForceUpdateAllAabbs = ForceUpdateAllAabbs
            };
        }

        public CollisionDispatcher Dispatcher { get; }
        public DbvtBroadphase Broadphase { get; }
        public DiscreteDynamicsWorld DynamicsWorld { get; }

        public void Dispose()
        {
            _solver.Dispose();
            _configuration.Dispose();
            Dispatcher.Dispose();
            Broadphase.Dispose();
            DynamicsWorld.Dispose();
        }
    }
}