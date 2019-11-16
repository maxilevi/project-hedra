using System;
using BulletSharp;
using BulletSharp.Math;

namespace Hedra.Engine.Bullet
{
    public class BulletWorld : IDisposable
    {
        public CollisionDispatcher Dispatcher { get; }
        public DbvtBroadphase Broadphase { get; }
        public DiscreteDynamicsWorld DynamicsWorld { get; }
        
        public BulletWorld(float Gravity, bool ForceUpdateAllAabbs = true)
        {
            var configuration = new DefaultCollisionConfiguration();
            Dispatcher = new CollisionDispatcher(configuration);
            Broadphase = new DbvtBroadphase();
            var solver = new SequentialImpulseConstraintSolver();
            DynamicsWorld = new DiscreteDynamicsWorld(Dispatcher, Broadphase, solver, configuration)
            {
                DebugDrawer = new BulletDraw
                {
                    DebugMode = DebugDrawModes.DrawWireframe
                },
                Gravity = -Vector3.UnitY * Gravity,
                ForceUpdateAllAabbs = ForceUpdateAllAabbs
            };
        }
        
        public void Dispose()
        {
            Dispatcher.Dispose();
            Broadphase.Dispose();
            DynamicsWorld.Dispose();
        }
    }
}