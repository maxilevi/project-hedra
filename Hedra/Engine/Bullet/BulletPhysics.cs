using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using BulletSharp;
using BulletSharp.Math;
using Hedra.Core;
using Hedra.Engine.Core;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Game;
using Hedra.Rendering;
using CollisionShape = BulletSharp.CollisionShape;
using Vector2 = OpenTK.Vector2;

namespace Hedra.Engine.Bullet
{
    public delegate void OnContactEvent(CollisionObject Body0, CollisionObject Body1);
    public class BulletPhysics
    {
        public const int TerrainFilter = 64;
        private const bool EnableDebugMode = true;
        public static event OnContactEvent OnCollision;
        public static event OnContactEvent OnSeparation;
        private static object _chunkLock;
        private static object _bulletLock;
        private static object _bodyLock;
        private static object _dynamicBodiesLock;
        private static object _staticBodiesLock;
        private static Dictionary<Vector2, RigidBody[]> _chunkBodies;
        private static DiscreteDynamicsWorld _dynamicsWorld;
        private static CollisionDispatcher _dispatcher;
        private static DbvtBroadphase _broadphase;
        private static List<RigidBody> _bodies;
        private static HashSet<Vector2> _activeChunks;
        private static List<RigidBody> _dynamicBodies;
        private static List<RigidBody> _staticBodies;
        private static HashSet<Pair<CollisionObject, CollisionObject>> _currentPairs;
        private static HashSet<Pair<CollisionObject, CollisionObject>> _pairsLastUpdate;
        public const float Gravity = 10 * Generation.ChunkSystem.Chunk.BlockSize;
        public static int ObjectsInSimulation { get; private set; }

        public static void Load()
        {
            _chunkLock = new object();
            _bulletLock = new object();
            _bodyLock = new object();
            _dynamicBodiesLock = new object();
            _staticBodiesLock = new object();
            _activeChunks = new HashSet<Vector2>();
            _currentPairs = new HashSet<Pair<CollisionObject, CollisionObject>>();
            _pairsLastUpdate = new HashSet<Pair<CollisionObject, CollisionObject>>();
            lock(_chunkLock)
                _chunkBodies = new Dictionary<Vector2, RigidBody[]>();
            lock(_bodyLock)
                _bodies = new List<RigidBody>();
            lock(_dynamicBodiesLock)
                _dynamicBodies = new List<RigidBody>();
            lock(_staticBodiesLock)
                _staticBodies = new List<RigidBody>();
            var configuration = new DefaultCollisionConfiguration();
            _dispatcher = new CollisionDispatcher(configuration);
            _broadphase = new DbvtBroadphase();
            var solver = new SequentialImpulseConstraintSolver();
            _dynamicsWorld = new DiscreteDynamicsWorld(_dispatcher, _broadphase, solver, configuration)
            {
                DebugDrawer = new BulletDraw
                {
                    DebugMode = DebugDrawModes.DrawWireframe
                },
                Gravity = -Vector3.UnitY * Gravity
            };
        }

        public static void Update(float DeltaTime)
        {
            lock (_bulletLock)
            {
                CalculateActiveOffsets();
                UpdateActivations();
                _dynamicsWorld.StepSimulation(DeltaTime);
                CheckForCollisionEvents();
            }
        }

        private static void CalculateActiveOffsets()
        {
            _activeChunks.Clear();
            lock (_dynamicBodiesLock)
            {
                for (var i = 0; i < _dynamicBodies.Count; ++i)
                {
                    var offset = World.ToChunkSpace(_dynamicBodies[i].WorldTransform.Origin.Compatible());
                    var information = (PhysicsObjectInformation) _dynamicBodies[i].UserObject;
                    if (_chunkBodies.ContainsKey(offset))
                    {
                        if (!_activeChunks.Contains(offset))
                            _activeChunks.Add(offset);
                        
                        if(!information.IsInSimulation)
                            AddToSimulation(_dynamicBodies[i], information);
                        #if DEBUG
                        if (_dynamicBodies[i].WorldTransform.Origin.Y < -100)
                        {
                            _dynamicBodies[i].ClearForces();
                            _dynamicBodies[i].Translate(Vector3.UnitY * (-_dynamicBodies[i].WorldTransform.Origin.Y + 500));
                        }
                        #endif
                    }
                    else
                    {
                        /* Disable physics on objects that are in places where the ground has not loaded yet. */
                        if(information.IsInSimulation)
                            RemoveFromSimulation(_dynamicBodies[i], information);
                    }
                }
            }
        }

        private static void UpdateActivations()
        {
            lock (_staticBodiesLock)
            {
                for (var i = 0; i < _staticBodies.Count; ++i)
                {
                    var information = (PhysicsObjectInformation) _staticBodies[i].UserObject;
                    var offsets = information.StaticOffsets;
                    lock (_bulletLock)
                    {
                        var containsAny = offsets.Any(_activeChunks.Contains);
                        if (containsAny && !information.IsInSimulation)
                            AddToSimulation(_staticBodies[i], information);
                        else if (!containsAny && information.IsInSimulation)
                            RemoveFromSimulation(_staticBodies[i], information);
                    }
                }
            }
        }

        private static void RemoveFromSimulation(RigidBody Body, PhysicsObjectInformation Information)
        {
            Information.IsInSimulation = false;
            _dynamicsWorld.RemoveRigidBody(Body);
            ObjectsInSimulation--;
        }

        private static void AddToSimulation(RigidBody Body, PhysicsObjectInformation Information)
        {
            Information.IsInSimulation = true;
            _dynamicsWorld.AddRigidBody(Body, Information.Group, Information.Mask);
            ObjectsInSimulation++;
        }

        public static void Draw()
        {
            if (!GameSettings.DebugPhysics) return;
            lock (_bodyLock)
            {
                /*for (var i = 0; i < _bodies.Count; ++i)
                {
                    if ((_bodies[i].WorldTransform.Origin.Compatible() - Player.LocalPlayer.Instance.Position).Xz
                        .LengthSquared > 64 * 64) continue;
                    _dynamicsWorld.DebugDrawObject(_bodies[i].WorldTransform, _bodies[i].CollisionShape,
                        new Vector3(1, 1, 0));
                }*/

                for (var i = 0; i < _staticBodies.Count; ++i)
                {
                    var info = (PhysicsObjectInformation)_staticBodies[i].UserObject;
                    if (info.IsInSimulation)// && info.Name != null && (info.Name.Contains("Door") || info.Name.Contains("Terrain")))
                    {
                        var trans = _staticBodies[i].WorldTransform;
                        var col = new Vector3(1,0,0);
                        //_dynamicsWorld.DebugDrawer.DrawSphere(4, ref trans, ref col);
                        _dynamicsWorld.DebugDrawObject(_staticBodies[i].WorldTransform, _staticBodies[i].CollisionShape, new Vector3(1, 1, 0));
                    }
                    //_dynamicsWorld.DebugDrawObject(_staticBodies[i].WorldTransform, _staticBodies[i].CollisionShape, new Vector3(1, 1, 0));
                }
            }
        }

        public static void DrawObject(Matrix Transform, CollisionShape Object, OpenTK.Vector4 Color)
        {
            _dynamicsWorld.DebugDrawer.DebugMode = DebugDrawModes.DrawWireframe;
            _dynamicsWorld.DebugDrawObject(Transform, Object, Color.Xyz.Compatible());
        }

        public static void Raycast(ref Vector3 From, ref Vector3 To, RayResultCallback Callback)
        {
            lock (_bulletLock)
                _dynamicsWorld.RayTestRef(ref From, ref To, Callback);
        }

        public static RigidBody AddGroup(CollisionGroup Group)
        {
            var body = CreateShapesRigidbody(Group.Colliders);
            if (body != null)
            {
                Add(body, new PhysicsObjectInformation
                {
                    Group = CollisionFilterGroups.StaticFilter,
                    Mask = CollisionFilterGroups.AllFilter,
                    StaticOffsets = Group.Offsets
                });
            }
            return body;
        }

        public static void Add(RigidBody Body, PhysicsObjectInformation Information)
        {
            lock (_bulletLock)
            {
                Body.UserObject = Information;
                AddToSimulation(Body, Information);
                lock(_bodyLock)
                    _bodies.Add(Body);

                if (!Information.IsSensor)
                {
                    if (!Body.IsStaticObject)
                    {
                        lock (_dynamicBodiesLock)
                            _dynamicBodies.Add(Body);
                        Information.IsDynamic = true;
                    }
                    else
                    {
                        lock (_staticBodiesLock)
                            _staticBodies.Add(Body);
                        Information.IsStatic = true;
                        if (!Information.ValidStaticObject)
                            throw new ArgumentOutOfRangeException($"Tried to add an incomplete static object.");
                    }
                }
            }
        }
        
        public static void RemoveAndDispose(RigidBody Body)
        {
            lock (_bulletLock)
            {
                var information = (PhysicsObjectInformation) Body.UserObject;
                lock(_bodyLock)
                    _bodies.Remove(Body);
                if (information.IsInSimulation)
                {
                    RemoveFromSimulation(Body, information);
                }
                if (information.IsDynamic)
                {
                    lock (_dynamicBodiesLock)
                        _dynamicBodies.Remove(Body);
                }
                if(information.IsStatic)
                {
                    lock (_staticBodiesLock)
                        _staticBodies.Remove(Body);
                    
                }
                if (Body.CollisionShape is BvhTriangleMeshShape bvhTriangleMeshShape)
                {
                    bvhTriangleMeshShape.MeshInterface.Dispose();
                }
                else if (Body.CollisionShape is CompoundShape compoundShape)
                {
                    for (var i = 0; i < compoundShape.NumChildShapes; ++i)
                    {
                        compoundShape.GetChildShape(i).Dispose();
                    }
                }
                Body.MotionState.Dispose();
                Body.CollisionShape.Dispose();
                Body.Dispose();
            }
        }
        
        public static void AddChunk(Vector2 Offset, VertexData Mesh, PhysicsSystem.CollisionShape[] Shapes)
        {
            lock (_bulletLock)
            {
                if(Mesh.IsEmpty) return;
                var bodies = new List<Pair<RigidBody, PhysicsObjectInformation>>
                {
                    new Pair<RigidBody, PhysicsObjectInformation>(CreateTerrainRigidbody(Offset, Mesh), new PhysicsObjectInformation
                    {
                        Group = CollisionFilterGroups.DebrisFilter,
                        Mask = CollisionFilterGroups.AllFilter,
                        Name = $"Terrain ({Offset.X}, {Offset.Y})",
                        StaticOffsets = new []{Offset}
                    }),
                };
                var shape = CreateShapesRigidbody(Shapes);
                if (shape != null)
                {
                    bodies.Add(new Pair<RigidBody, PhysicsObjectInformation>(shape, new PhysicsObjectInformation
                    {
                        Group = CollisionFilterGroups.StaticFilter,
                        Mask = CollisionFilterGroups.AllFilter,
                        Name = $"Static Objects on ({Offset.X}, {Offset.Y})",
                        StaticOffsets = new CollisionGroup(Shapes).Offsets
                    }));
                }

                RemoveChunk(Offset);
                lock (_chunkLock)
                    _chunkBodies.Add(Offset, bodies.Select(B => B.One).ToArray());
                for (var i = 0; i < bodies.Count; ++i)
                {
                    Add(bodies[i].One, bodies[i].Two);
                }
            }
        }
        
        public static void RemoveChunk(Vector2 Offset)
        {
            lock (_bulletLock)
            {
                lock (_chunkLock)
                {
                    if (!_chunkBodies.ContainsKey(Offset)) return;
                    var bodies = _chunkBodies[Offset];
                    for (var i = 0; i < bodies.Length; ++i)
                    {
                        RemoveAndDispose(bodies[i]);
                    }

                    _chunkBodies.Remove(Offset);
                }
            }
        }

        public static bool HasChunk(Vector2 Offset)
        {
            lock (_chunkLock)
            {
                return _chunkBodies.ContainsKey(Offset);
            }
        }

        private static RigidBody CreateShapesRigidbody(PhysicsSystem.CollisionShape[] Shapes)
        {
            if (Shapes.Length == 0) return null;
            var offset = (Shapes.Select(S => S.BroadphaseCenter).Aggregate((S1,S2) => S1 + S2) / Shapes.Length).Compatible();
            var triangleMesh = new TriangleIndexVertexArray();
            for (var i = 0; i < Shapes.Length; ++i)
            {
                if (Shapes[i].Indices.Length % 3 != 0 || Shapes[i].Indices.Length == 0)
                    throw new ArgumentOutOfRangeException();
                triangleMesh.AddIndexedMesh(CreateIndexedMesh(Shapes[i].Indices, Shapes[i].Vertices.Select(V => V - offset.Compatible()).ToArray()));
            }
            var body = CreateStaticRigidbody(new BvhTriangleMeshShape(triangleMesh, true));
            body.Translate(offset);
            return body;

        }

        private static RigidBody CreateTerrainRigidbody(Vector2 Offset, VertexData Mesh)
        {
            var shape = CreateTriangleShape(Mesh.Indices, Mesh.Vertices);
            var body = CreateStaticRigidbody(shape);
            body.Translate(Offset.ToVector3().Compatible());
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
        
        public static bool Raycast(Vector3 Source, Vector3 End)
        {
            lock (_bulletLock)
            {
                var src = Source;
                var end = End;
                var callback = new ClosestRayResultCallback(ref src, ref end);
                try
                {
                    BulletPhysics.Raycast(ref Source, ref End, callback);
                    return callback.HasHit;
                }
                finally
                {
                    callback.Dispose();
                }
            }
        }
        
        public static void ConvexSweepTest(ConvexShape Shape, ref Matrix From, ref Matrix To, ref ClosestConvexResultCallback Callback)
        {
            lock (_bulletLock)
            {
                _dynamicsWorld.ConvexSweepTestRef(Shape, ref From, ref To, Callback);
            }
        }

        public static bool Collides(Vector3 Offset, RigidBody Body)
        {
            lock (_bulletLock)
            {
                var callback = new ContactTestResultCallback();
                try
                {
                    Body.Translate(Offset);
                    _dynamicsWorld.ContactTest(Body, callback);
                    return callback.HasHit;
                }
                finally
                {
                    Body.Translate(-Offset);
                    callback.Dispose();
                }
            }
        }

        public static void ResetCallback(ClosestRayResultCallback Callback)
        {
            Callback.ClosestHitFraction = 1;
            Callback.CollisionObject = null;
            Callback.CollisionFilterGroup = (int)CollisionFilterGroups.DefaultFilter;
            Callback.CollisionFilterMask = (int)CollisionFilterGroups.AllFilter;
        }

        public static CompoundShape ShapeFrom(Box Dimensions)
        {
            var bodyShape = new CompoundShape();
            var cube = new BoxShape(Dimensions.Size.Compatible() * .5f);
            bodyShape.AddChildShape(
                Matrix.Translation(Vector3.UnitY * -cube.HalfExtentsWithoutMargin.Y),
                cube
            );
            return bodyShape;
        }

        public static int RigidbodyCount
        {
            get
            {
                lock (_bodies)
                    return _bodies.Count;
            }
        }
    }
}