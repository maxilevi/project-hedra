using System.Collections.Generic;
using BulletSharp;
using Hedra.Core;
using Hedra.Game;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.BulletPhysics
{
    public class BulletPhysics
    {
        private static object _chunkLock;
        private static Dictionary<Vector2, RigidBody> _chunkBodies;
        private static DiscreteDynamicsWorld _dynamicsWorld;
        
        public static void Load()
        {
            _chunkLock = new object();
            lock(_chunkLock)
                _chunkBodies = new Dictionary<Vector2, RigidBody>();
            var configuration = new DefaultCollisionConfiguration();
            var dispatcher = new CollisionDispatcher(configuration);
            var broadphase = new DbvtBroadphase();
            var solver = new SequentialImpulseConstraintSolver();
            _dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, configuration)
            {
                DebugDrawer = new BulletDraw
                {
                    DebugMode = DebugDrawModes.DrawAabb
                }
            };
        }

        public static void Update()
        {
            _dynamicsWorld.StepSimulation(Time.DeltaTime);
        }

        public static void Add(RigidBody Body)
        {
            _dynamicsWorld.AddRigidBody(Body);
        }

        public static void Draw()
        {
            if (GameSettings.DebugView)
            {
                _dynamicsWorld.DebugDrawer.DebugMode = DebugDrawModes.DrawAabb;
                _dynamicsWorld.DebugDrawWorld();
            }
        }

        public static void Remove(RigidBody Body)
        {
            _dynamicsWorld.RemoveRigidBody(Body);
            Body.Dispose();
        }
        
        public static void AddChunk(Vector2 Offset, VertexData Mesh)
        {
            var average = Mesh.Vertices.ToArray().Average();
            var triangleMesh = new TriangleMesh(true, false);
            for (var i = 0; i < Mesh.Indices.Count; i+=3)
            {
                triangleMesh.AddTriangle(
                    Mesh.Vertices[(int)Mesh.Indices[i]].Compatible() - average.Compatible(),
                    Mesh.Vertices[(int)Mesh.Indices[i+1]].Compatible() - average.Compatible(),
                    Mesh.Vertices[(int)Mesh.Indices[i+2]].Compatible() - average.Compatible()
                );
            }
            var shape = new BvhTriangleMeshShape(triangleMesh, true);
            var body = new RigidBody(new RigidBodyConstructionInfo(0, new DefaultMotionState(), shape));
            body.Translate(average.Compatible());
            RemoveChunk(Offset);
            lock (_chunkLock) 
                _chunkBodies.Add(Offset, body);
            Add(body);
        }

        public static void RemoveChunk(Vector2 Offset)
        {
            lock (_chunkLock)
            {
                if (_chunkBodies.ContainsKey(Offset))
                {
                    Remove(_chunkBodies[Offset]);
                    _chunkBodies.Remove(Offset);
                }
            }
        }
    }
}