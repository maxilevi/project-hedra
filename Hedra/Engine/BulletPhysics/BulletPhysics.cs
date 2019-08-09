using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BulletSharp;
using BulletSharp.Math;
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
        private static Dictionary<Vector2, RigidBody[]> _chunkBodies;
        private static DiscreteDynamicsWorld _dynamicsWorld;
        private static CollisionDispatcher _dispatcher;
        private static HashSet<Pair<CollisionObject, CollisionObject>> _currentPairs;
        private static HashSet<Pair<CollisionObject, CollisionObject>> _pairsLastUpdate;
        public const float Gravity = 10 * Generation.ChunkSystem.Chunk.BlockSize;
        
        public static void Load()
        {
            _currentPairs = new HashSet<Pair<CollisionObject, CollisionObject>>();
            _pairsLastUpdate = new HashSet<Pair<CollisionObject, CollisionObject>>();
            _chunkLock = new object();
            lock(_chunkLock)
                _chunkBodies = new Dictionary<Vector2, RigidBody[]>();
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
                _dynamicsWorld.DebugDrawer.DebugMode = DebugDrawModes.DrawWireframe;
                var offset = World.ToChunkSpace(Engine.Player.LocalPlayer.Instance.Position);
                if (_chunkBodies.ContainsKey(offset))
                {
                    var chunkObjects = _chunkBodies[offset];
                    if (chunkObjects.Length == 2)
                        _dynamicsWorld.DebugDrawObject(chunkObjects[1].WorldTransform, chunkObjects[1].CollisionShape, new Vector3(1, 1, 0));
                }
            }
        }

        public static void DrawObject(Matrix Transform, CollisionShape Object, OpenTK.Vector4 Color)
        {
            _dynamicsWorld.DebugDrawer.DebugMode = DebugDrawModes.DrawWireframe;
            _dynamicsWorld.DebugDrawObject(Transform, Object, Color.Xyz.Compatible());
        }

        public static void Remove(RigidBody Body)
        {
            _dynamicsWorld.RemoveRigidBody(Body);
            Body.CollisionShape.Dispose();
            Body.Dispose();
        }

        public static void Raycast(Vector3 From, Vector3 To, RayResultCallback Callback)
        {
            _dynamicsWorld.RayTestRef(ref From, ref To, Callback);
        }
        
        public static void AddChunk(Vector2 Offset, VertexData Mesh, PhysicsSystem.CollisionShape[] Shapes)
        {
            var bodies = new List<RigidBody>
            {
                CreateTerrainRigidbody(Offset, Mesh),
            };
            var shape = CreateShapesRigidbody(Offset, Shapes);
            if(shape != null)
                bodies.Add(shape);

            RemoveChunk(Offset);
            lock (_chunkLock) 
                _chunkBodies.Add(Offset, bodies.ToArray());
            for (var i = 0; i < bodies.Count; ++i)
            {
                Add(bodies[i]);
            }
        }

        private static RigidBody CreateShapesRigidbody(Vector2 Offset, PhysicsSystem.CollisionShape[] Shapes)
        {
            if (Shapes.Length != 0)
            {
                var triangleMesh = new TriangleIndexVertexArray();
                for (var i = 0; i < Shapes.Length; ++i)
                {
                    triangleMesh.AddIndexedMesh(CreateIndexedMesh(Shapes[i].Indices, Shapes[i].Vertices));
                }
                var body = CreateStaticRigidbody(new BvhTriangleMeshShape(triangleMesh, true));
                body.UserObject = $"Static Objects on ({Offset.X}, {Offset.Y})";
                return body;
            }

            return null;
        }

        private static RigidBody CreateTerrainRigidbody(Vector2 Offset, VertexData Mesh)
        {
            var shape = CreateTriangleShape(Mesh.Indices, Mesh.Vertices);
            var body = CreateStaticRigidbody(shape);
            body.Translate(Offset.ToVector3().Compatible());
            body.UserObject = $"Terrain ({Offset.X}, {Offset.Y})";
            return body;
        }

        private static RigidBody CreateStaticRigidbody(CollisionShape Shape)
        {
            using (var bodyInfo = new RigidBodyConstructionInfo(0, new DefaultMotionState(), Shape))
            {
                var body = new RigidBody(bodyInfo);
                body.CollisionFlags |= CollisionFlags.StaticObject;
                body.Friction = 0;
                return body;
            }
        }

        private static BvhTriangleMeshShape CreateTriangleShape(ICollection<uint> Indices, ICollection<OpenTK.Vector3> Vertices)
        {
            var triangleMesh = new TriangleIndexVertexArray();
            var indexedMesh = CreateIndexedMesh(Indices, Vertices);
            triangleMesh.AddIndexedMesh(indexedMesh);
            return new BvhTriangleMeshShape(triangleMesh, true);
        }

        private static IndexedMesh CreateIndexedMesh(ICollection<uint> Indices, ICollection<OpenTK.Vector3> Vertices)
        {
            var indexedMesh = new IndexedMesh();
            indexedMesh.Allocate(Indices.Count / 3, Vertices.Count);
            indexedMesh.SetData(
                Indices.Select(I => (int) I).ToList(),
                Vertices.SelectMany(V => new[] {V.X, V.Y, V.Z}).ToList()
            );
            return indexedMesh;
        }

        public static void RemoveChunk(Vector2 Offset)
        {
            lock (_chunkLock)
            {
                if (_chunkBodies.ContainsKey(Offset))
                {
                    var bodies = _chunkBodies[Offset];
                    for (var i = 0; i < bodies.Length; ++i)
                    {
                        Remove(bodies[i]);
                    }
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