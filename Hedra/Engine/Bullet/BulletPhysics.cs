using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BulletSharp;
using BulletSharp.Math;
using Hedra.Core;
using Hedra.Engine.Core;
using Hedra.Engine.IO;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering;
using Hedra.Game;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.Framework;
using CollisionShape = BulletSharp.CollisionShape;
using Vector2 = System.Numerics.Vector2;

namespace Hedra.Engine.Bullet
{
    public delegate void OnContactEvent(CollisionObject Body0, CollisionObject Body1);

    public delegate void OnRigidbodyEvent(RigidBody Body);
    public static class BulletPhysics
    {
        public static int UsedBytes { get; set; }
        public const CollisionFilterGroups TerrainFilter = CollisionFilterGroups.DebrisFilter;
        public static event OnRigidbodyEvent OnRigidbodyReAdded;
        public static event OnContactEvent OnCollision;
        public static event OnContactEvent OnSeparation;
        private static object _chunkLock;
        private static object _bulletLock;
        private static object _bodyLock;
        private static object _dynamicBodiesLock;
        private static object _staticBodiesLock;
        private static object _sensorsLock;
        private static List<RigidBody> _sensors;
        private static Dictionary<Vector2, RigidBody[]> _chunkBodies;
        private static DiscreteDynamicsWorld _dynamicsWorld;
        private static CollisionDispatcher _dispatcher;
        private static DbvtBroadphase _broadphase;
        private static List<RigidBody> _bodies;
        private static HashSet<Vector2> _activeChunks;
        private static HashSet<Vector2> _lastActiveChunks;
        private static List<RigidBody> _dynamicBodies;
        private static Dictionary<Vector2, List<RigidBody>> _staticBodies;
        private static HashSet<Pair<CollisionObject, CollisionObject>> _currentPairs;
        private static HashSet<Pair<CollisionObject, CollisionObject>> _pairsLastUpdate;
        public const float Gravity = 10 * Generation.ChunkSystem.Chunk.BlockSize;
        public static int ObjectsInSimulation { get; private set; }

        public static void Load()
        {
            _sensorsLock = new object();
            _chunkLock = new object();
            _bulletLock = new object();
            _bodyLock = new object();
            _dynamicBodiesLock = new object();
            _staticBodiesLock = new object();
            _activeChunks = new HashSet<Vector2>();
            _currentPairs = new HashSet<Pair<CollisionObject, CollisionObject>>();
            _pairsLastUpdate = new HashSet<Pair<CollisionObject, CollisionObject>>();
            _lastActiveChunks = new HashSet<Vector2>();
            lock(_chunkLock)
                _chunkBodies = new Dictionary<Vector2, RigidBody[]>();
            lock(_bodyLock)
                _bodies = new List<RigidBody>();
            lock(_dynamicBodiesLock)
                _dynamicBodies = new List<RigidBody>();
            lock(_staticBodiesLock)
                _staticBodies = new Dictionary<Vector2, List<RigidBody>>();
            lock(_sensorsLock)
                _sensors = new List<RigidBody>();
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
                Gravity = -Vector3.UnitY * Gravity,
                ForceUpdateAllAabbs = false
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
            /* Switch references */
            var temp = _lastActiveChunks;
            _lastActiveChunks = _activeChunks;
            _activeChunks = temp;
            
            _activeChunks.Clear();
            lock (_dynamicBodiesLock)
                AddOffsetsFromDynamicObjects();
            lock (_sensorsLock)
                DisableSensorsWhenNecessary();
        }

        private static void DisableSensorsWhenNecessary()
        {
            for (var i = 0; i < _sensors.Count; ++i)
            {
                var offset = World.ToChunkSpace(_sensors[i].WorldTransform.Origin.Compatible());
                var information = (PhysicsObjectInformation) _sensors[i].UserObject;
                if (_chunkBodies.ContainsKey(offset))
                {
                    if (!information.IsInSimulation)
                        AddToSimulation(_sensors[i], information);
                }
                else
                {
                    if (information.IsInSimulation)
                        RemoveFromSimulation(_sensors[i], information);
                }
            }
        }

        public static void ApplyMaskChanges(RigidBody Body)
        {
            var information = (PhysicsObjectInformation) Body.UserObject;
            if (information.IsInSimulation)
            {
                lock (_bulletLock)
                {
                    RemoveFromSimulation(Body, information);
                    AddToSimulation(Body, information);
                }
            }
        }
        
        private static void AddOffsetsFromDynamicObjects()
        {
            for (var i = 0; i < _dynamicBodies.Count; ++i)
            {
                var offset = World.ToChunkSpace(_dynamicBodies[i].WorldTransform.Origin.Compatible());
                var information = (PhysicsObjectInformation) _dynamicBodies[i].UserObject;
                if (_chunkBodies.ContainsKey(offset))
                {
                    if (!_activeChunks.Contains(offset))
                        _activeChunks.Add(offset);

                    if (!information.IsInSimulation)
                        AddToSimulation(_dynamicBodies[i], information);
                    AssertIsNotFailingThroughFloor(_dynamicBodies[i]);
                }
                else
                {
                    /* Disable physics on objects that are in places where the ground has not loaded yet. */
                    if (information.IsInSimulation)
                    {
                        RemoveFromSimulation(_dynamicBodies[i], information);
                    }
                }
            }
        }

        private static void AssertIsNotFailingThroughFloor(RigidBody Body)
        {
            var position = Body.WorldTransform.Origin;
            if (position.Y < 0)
            {
                var information = (PhysicsObjectInformation) Body.UserObject;
                Log.WriteLine($"'{(information.IsEntity ? information.Entity.Name : information.Name)}' fell through the world at '{position}'. Fixing its height...");
                Body.ClearForces();
                if (information.IsEntity)
                {
                    information.Entity.Physics.ResetFall();
                    information.Entity.Physics.ResetVelocity();
                    information.Entity.Position = new System.Numerics.Vector3(information.Entity.Position.X, Physics.HeightAtPosition(position.Compatible()), information.Entity.Position.Z);
                }
            }
        }

        private static void UpdateActivations()
        {
            lock (_staticBodiesLock)
            {
                foreach (var offset in _lastActiveChunks)
                {
                    if(!_activeChunks.Contains(offset) && _staticBodies.ContainsKey(offset))
                        UpdateActivationsOfBodies(_staticBodies[offset], false);
                }
                foreach (var offset in _activeChunks)
                {
                    if(_staticBodies.ContainsKey(offset))
                        UpdateActivationsOfBodies(_staticBodies[offset], true);
                }
            }
        }
        
        private static void UpdateActivationsOfBodies(List<RigidBody> Bodies, bool Add)
        {
            for (var i = 0; i < Bodies.Count; ++i)
            {
                var information = (PhysicsObjectInformation) Bodies[i].UserObject;
                lock (_bulletLock)
                {
                    if (Add)
                    {
                        if (!information.IsInSimulation)
                            AddToSimulation(Bodies[i], information);
                        _dynamicsWorld.UpdateSingleAabb(Bodies[i]);
                    }
                    else
                    {
                        if (information.IsInSimulation)
                            RemoveFromSimulation(Bodies[i], information);
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
            OnRigidbodyReAdded?.Invoke(Body);
            ObjectsInSimulation++;
        }

        public static void Draw()
        {
            if (!GameSettings.DebugPhysics) return;
            lock (_bodyLock)
            {
                for (var i = 0; i < _bodies.Count; ++i)
                {
                    if ((_bodies[i].WorldTransform.Origin.Compatible() - Player.LocalPlayer.Instance.Position).Xz().LengthSquared() > 64 * 64) continue;
                    var info = (PhysicsObjectInformation) _bodies[i].UserObject;
                    if (info.IsInSimulation)
                    {
                        _dynamicsWorld.DebugDrawObject(_bodies[i].WorldTransform, _bodies[i].CollisionShape, new Vector3(1, 1, 0));
                    }
                }/*
                for (var i = 0; i < _bodies.Count; ++i)
                {
                    //if ((_bodies[i].WorldTransform.Origin.Compatible() - Player.LocalPlayer.Instance.Position).Xz().LengthSquared() > 64 * 64) continue;
                    var info = (PhysicsObjectInformation) _bodies[i].UserObject;
                    if (info.IsInSimulation)
                        _dynamicsWorld.DebugDrawObject(_bodies[i].WorldTransform, _bodies[i].CollisionShape, new Vector3(1, 1, 0));
                }*/
            }
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
                if (!Information.IsSensor)
                {
                    if (!Body.IsStaticObject)
                        AddDynamic(Body, Information);
                    else
                        AddStatic(Body, Information);
                }
                else
                {
                    AddSensor(Body);
                }
                lock(_bodyLock)
                    _bodies.Add(Body);
                
                UsedBytes += Information.UsedBytes;
            }
        }

        private static void AddSensor(RigidBody Body)
        {
            lock(_sensorsLock)
                _sensors.Add(Body);
        }

        private static void AddStatic(RigidBody Body, PhysicsObjectInformation Information)
        {
            Information.IsStatic = true;
            if (!Information.ValidStaticObject)
                throw new ArgumentOutOfRangeException($"Tried to add an incomplete static object.");
            lock (_staticBodiesLock)
            {
                var offsets = Information.StaticOffsets;
                for (var i = 0; i < offsets.Length; ++i)
                {
                    if (!_staticBodies.ContainsKey(offsets[i]))
                        _staticBodies.Add(offsets[i], new List<RigidBody>());
                    _staticBodies[offsets[i]].Add(Body);
                }
            }
        }

        private static void AddDynamic(RigidBody Body, PhysicsObjectInformation Information)
        {
            Information.IsDynamic = true;
            lock (_dynamicBodiesLock)
                _dynamicBodies.Add(Body);
        }
        
        public static void RemoveAndDispose(RigidBody Body)
        {
            if(Body.IsDisposed) return;
            lock (_bulletLock)
            {
                var information = (PhysicsObjectInformation) Body.UserObject;
                lock(_bodyLock)
                    _bodies.Remove(Body);
                
                if (information.IsInSimulation)
                    RemoveFromSimulation(Body, information);

                if (information.IsDynamic)
                    DisposeDynamic(Body);
                
                if(information.IsStatic)
                    DisposeStatic(Body, information);

                if (information.IsSensor)
                    DisposeSensor(Body);

                #if DEBUG
                if (_sensors.Contains(Body) || _dynamicBodies.Contains(Body))
                {
                    int a = 0;
                }
#endif
                UsedBytes -= information.UsedBytes;
                DisposeBody(Body);
            }
        }

        private static void DisposeSensor(RigidBody Body)
        {
            lock (_sensorsLock)
                _sensors.Remove(Body);
        }

        private static void DisposeDynamic(RigidBody Body)
        {
            lock (_dynamicBodiesLock)
                _dynamicBodies.Remove(Body);
        }

        private static void DisposeStatic(RigidBody Body, PhysicsObjectInformation Information)
        {
            lock (_staticBodiesLock)
            {
                var offsets = Information.StaticOffsets;
                for (var i = 0; i < offsets.Length; ++i)
                {
                    _staticBodies[offsets[i]].Remove(Body);
                    if (_staticBodies[offsets[i]].Count == 0)
                        _staticBodies.Remove(offsets[i]);
                }
            }
        }

        
        public static void DisposeBody(RigidBody Body)
        {
            switch (Body.CollisionShape)
            {
                case BvhTriangleMeshShape bvhTriangleMeshShape:
                    var meshArray = (TriangleIndexVertexArray)bvhTriangleMeshShape.MeshInterface;
                    var count = meshArray.IndexedMeshArray.Count;
                    for (var i = 0; i < count; ++i)
                    {
                        meshArray.IndexedMeshArray[i].Dispose();
                    }
                    meshArray.Dispose();
                    break;
                case CompoundShape compoundShape:
                {
                    for (var i = 0; i < compoundShape.NumChildShapes; ++i)
                    {
                        compoundShape.GetChildShape(i).Dispose();
                    }
                    break;
                }
                case BoxShape boxShape:
                    boxShape.Dispose();
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException($"Unknown collision shape of type '{Body.CollisionShape.ShapeType}' and object type '{Body.CollisionShape.GetType()}' found");
            }
            Body.MotionState.Dispose();
            Body.Dispose();
        }
        
        public static void AddChunk(Vector2 Offset, NativeVertexData Mesh, PhysicsSystem.CollisionShape[] Shapes, Vector2[] Offsets)
        {
            lock (_bulletLock)
            {
                if(Mesh.IsEmpty)
                    throw new ArgumentOutOfRangeException("Cannot add an empty mesh");
                var bodies = new List<Pair<RigidBody, PhysicsObjectInformation>>
                {
                    new Pair<RigidBody, PhysicsObjectInformation>(CreateTerrainRigidbody(Offset, Mesh), new PhysicsObjectInformation
                    {
                        Group = TerrainFilter,
                        Mask = CollisionFilterGroups.AllFilter,
                        Name = $"Terrain ({Offset.X}, {Offset.Y})",
                        StaticOffsets = new []{Offset},
                        UsedBytes = Mesh.Vertices.Count * HedraSize.Vector3 + Mesh.Indices.Count * sizeof(uint)
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
                        StaticOffsets = Offsets,
                        UsedBytes = Shapes.Sum(S => S.SizeInBytes)
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

        private static RigidBody CreateShapesRigidbody(PhysicsSystem.CollisionShape[] Shapes)
        {
            if (Shapes.Length == 0) return null;
            var accumulator = new VertexData();
            for (var i = 0; i < Shapes.Length; ++i)
            {
                accumulator += new VertexData()
                {
                    Vertices = Shapes[i].Vertices.ToList(),
                    Indices = Shapes[i].Indices.ToList()
                };
            }
            accumulator.UniqueVertices();
            var uniqueShape = new PhysicsSystem.CollisionShape(accumulator);
            var offset = uniqueShape.BroadphaseCenter.Compatible();//(Shapes.Select(S => S.BroadphaseCenter).Aggregate((S1,S2) => S1 + S2) / Shapes.Length).Compatible();
            var triangleMesh = new TriangleIndexVertexArray();
            var vertCount = 0;
            //for (var i = 0; i < Shapes.Length; ++i)
            {
                AssertValidShape(uniqueShape);
                triangleMesh.AddIndexedMesh(CreateIndexedMesh(uniqueShape.Indices, uniqueShape.Vertices.Select(V => V - offset.Compatible()).ToArray()));
                vertCount += uniqueShape.Vertices.Length;
            }
            
            var shape = new BvhTriangleMeshShape(triangleMesh, true);
#if DEBUG
            if (float.IsInfinity(shape.LocalAabbMax.LengthSquared))
            {
                int a = 0;
            }
#endif
            var body = CreateStaticRigidbody(shape);
            body.Translate(offset);
            return body;
        }

        private static void AssertValidShape(PhysicsSystem.CollisionShape Shape)
        {
            for (var k = 0; k < Shape.Indices.Length; ++k)
            {
                var index = Shape.Indices[k];
                if(Shape.Indices[k] >= Shape.Vertices.Length)
                    throw new ArgumentOutOfRangeException();
            }
            for (var k = 0; k < Shape.Vertices.Length; ++k)
            {
                if(Shape.Vertices[k].IsInvalid())
                    throw new ArgumentOutOfRangeException();
            }
            if (Shape.Indices.Length % 3 != 0 || Shape.Indices.Length == 0 || Shape.Vertices.Length == 0)
                throw new ArgumentOutOfRangeException();
        }

        private static RigidBody CreateTerrainRigidbody(Vector2 Offset, NativeVertexData Mesh)
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

        public static BvhTriangleMeshShape CreateTriangleShape(ICollection<uint> Indices, ICollection<System.Numerics.Vector3> Vertices)
        {
            var triangleMesh = new TriangleIndexVertexArray();
            var indexedMesh = CreateIndexedMesh(Indices, Vertices);
            triangleMesh.AddIndexedMesh(indexedMesh);
            return new BvhTriangleMeshShape(triangleMesh, true);
        }

        private static IndexedMesh CreateIndexedMesh(ICollection<uint> Indices, ICollection<System.Numerics.Vector3> Vertices)
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
            foreach (var pair in _pairsLastUpdate)
            {
                if(!_currentPairs.Contains(pair))
                    OnSeparation?.Invoke(pair.One, pair.Two);
            }

            /* Switch the references to avoid copying data */
            var temp = _pairsLastUpdate;
            _pairsLastUpdate = _currentPairs;
            _currentPairs = temp;
        }
        
        public static RayResult Raycast(Vector3 Source, Vector3 End, CollisionFilterGroups Mask)
        {
            lock (_bulletLock)
            {
                var src = Source;
                var end = End;
                var callback = new ClosestRayResultCallback(ref src, ref end)
                {
                    CollisionFilterMask = (int) Mask
                };
                try
                {
                    BulletPhysics.Raycast(ref Source, ref End, callback);
                    return new RayResult(callback.HasHit, callback.HitPointWorld);
                }
                finally
                {
                    callback.Dispose();
                }
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

        private static void BaseResetCallback(RayResultCallback Callback)
        {
            Callback.ClosestHitFraction = 1;
            Callback.CollisionObject = null;
            Callback.CollisionFilterGroup = (int)CollisionFilterGroups.DefaultFilter;
            Callback.CollisionFilterMask = (int)CollisionFilterGroups.AllFilter;
        }

        public static void ResetCallback(ClosestRayResultCallback Callback)
        {
            BaseResetCallback(Callback);
        }

        public static void ResetCallback(AllHitsRayResultCallback Callback)
        {
            BaseResetCallback(Callback);
            Callback.CollisionObjects.Clear();
            Callback.HitFractions.Clear();
            Callback.HitNormalWorld.Clear();
            Callback.HitPointWorld.Clear();
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
    
    public struct RayResult
    {
        public bool HasHit { get; }
        public Vector3 HitPointWorld { get; }

        public RayResult(bool HasHit, Vector3 HitPointWorld)
        {
            this.HasHit = HasHit;
            this.HitPointWorld = HitPointWorld;
        }
    }
}