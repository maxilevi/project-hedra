using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BulletSharp;
using Hedra.Core;
using Hedra.Engine.Core;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Game;
using Hedra.Rendering;
using Vector2 = OpenTK.Vector2;

namespace Hedra.Engine.BulletPhysics
{
    public delegate void OnContactEvent(CollisionObject Body0, CollisionObject Body1);
    public class BulletPhysics
    {
        public static event OnContactEvent OnCollision;
        public static event OnContactEvent OnSeparation;
        private static object _chunkLock;
        private static Dictionary<Vector2, RigidBody> _chunkBodies;
        private static DiscreteDynamicsWorld _dynamicsWorld;
        private static CollisionDispatcher _dispatcher;
        private static HashSet<Pair<CollisionObject, CollisionObject>> _currentPairs;
        private static HashSet<Pair<CollisionObject, CollisionObject>> _pairsLastUpdate;
        public const float Gravity = 10 * Chunk.BlockSize;
        
        public static void Load()
        {
            _currentPairs = new HashSet<Pair<CollisionObject, CollisionObject>>();
            _pairsLastUpdate = new HashSet<Pair<CollisionObject, CollisionObject>>();
            _chunkLock = new object();
            lock(_chunkLock)
                _chunkBodies = new Dictionary<Vector2, RigidBody>();
            var configuration = new DefaultCollisionConfiguration();
            _dispatcher = new CollisionDispatcher(configuration);
            var broadphase = new DbvtBroadphase();
            var solver = new SequentialImpulseConstraintSolver();
            _dynamicsWorld = new DiscreteDynamicsWorld(_dispatcher, broadphase, solver, configuration)
            {
                DebugDrawer = new BulletDraw(),
                Gravity = -Vector3.UnitY * Gravity
            };
        }

        public static void Update()
        {
            _dynamicsWorld.StepSimulation(Time.DeltaTime);
            CheckForCollisionEvents();
        }

        public static void Add(RigidBody Body)
        {
            _dynamicsWorld.AddRigidBody(Body);
        }

        public static void Draw()
        {
            if (GameSettings.DebugPhysics)
            {
                _dynamicsWorld.DebugDrawer.DebugMode = DebugDrawModes.DrawContactPoints;
                _dynamicsWorld.DebugDrawWorld();
            }
        }

        public static void DrawObject(Matrix Transform, CollisionShape Object, Color Color)
        {
            _dynamicsWorld.DebugDrawer.DebugMode = DebugDrawModes.DrawWireframe;
            _dynamicsWorld.DebugDrawObject(Transform, Object, Color);
        }

        public static void Remove(RigidBody Body)
        {
            _dynamicsWorld.RemoveRigidBody(Body);
            Body.Dispose();
        }

        public static void Raycast(Vector3 From, Vector3 To, RayResultCallback Callback)
        {
            _dynamicsWorld.RayTestRef(ref From, ref To, Callback);
        }
        
        public static void AddChunk(Vector2 Offset, VertexData Mesh)
        {
            var indexedMesh = new IndexedMesh();
            indexedMesh.Allocate(Mesh.Indices.Count / 3, Mesh.Vertices.Count, 3 * sizeof(uint), Vector3.SizeInBytes, PhyScalarType.Int32, PhyScalarType.Single);
            using (var stream = indexedMesh.LockVerts())
            {
                for (var i = 0; i < Mesh.Vertices.Count; ++i)
                {
                    stream.Write(Mesh.Vertices[i]);
                }
            }
            var indicesData = indexedMesh.TriangleIndices;
            for (var i = 0; i < Mesh.Indices.Count; ++i)
            {
                indicesData[i] = (int) Mesh.Indices[i];
            }

            var triangleMesh = new TriangleIndexVertexArray();
            triangleMesh.AddIndexedMesh(indexedMesh);
            var shape = new BvhTriangleMeshShape(triangleMesh, true);
            var body = new RigidBody(new RigidBodyConstructionInfo(0, new DefaultMotionState(), shape));
            body.Translate(Offset.ToVector3().Compatible());
            body.CollisionFlags |= CollisionFlags.StaticObject;
            body.UserObject = $"Terrain ({Offset.X}, {Offset.Y})";
            body.Friction = 0;
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
        
        private static void CheckForCollisionEvents()
        {
            _currentPairs.Clear();
            for (var i = 0; i < _dispatcher.NumManifolds; ++i)
            {
                var manifold = _dispatcher.GetManifoldByIndexInternal(i);
                if (manifold.NumContacts <= 0) continue;
                
                var pair = new Pair<CollisionObject, CollisionObject>(manifold.Body0, manifold.Body1);
                _currentPairs.Add(pair);
                if (!_pairsLastUpdate.Contains(pair))
                {
                    OnCollision?.Invoke(pair.One, pair.Two);
                }
            }
            var removedPairs = _pairsLastUpdate.Except(_currentPairs).ToArray();
            for(var i = 0; i < removedPairs.Length; ++i)
            {
                OnSeparation?.Invoke(removedPairs[i].One, removedPairs[i].Two);
            }

            /* Switch the references to avoid copying data */
            var temp = _pairsLastUpdate;
            _pairsLastUpdate = _currentPairs;
            _currentPairs = temp;
        }
    }
}