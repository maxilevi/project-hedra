using System.Collections.Generic;
using System.Linq;
using BulletSharp;
using BulletSharp.Math;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Frustum;
using Hedra.Game;
using Hedra.Rendering;
using OpenTK;
using Vector3 = OpenTK.Vector3;
using Vector4 = OpenTK.Vector4;

namespace Hedra.Engine.Bullet
{
    public class BulletFrustum
    {
        private readonly Vector4[] _unitCube = new Vector4[8]
        {
            new Vector4(-1, -1, -1, 1),
            new Vector4(-1, -1, 1, 1),
            new Vector4(-1, 1, -1, 1),
            new Vector4(1, -1, -1, 1),
            new Vector4(1, -1, 1, 1),
            new Vector4(-1, 1, 1, 1),
            new Vector4(1, 1, -1, 1),
            new Vector4(1, 1, 1, 1),
        };
        private readonly object _bulletLock;
        private readonly object _bodiesLock;
        private readonly FrustumContactTestCallback _callback;
        private readonly Dictionary<ICullable, RigidBody> _bodies;
        private readonly FrustumShape _frustumShape;
        private readonly RigidBody _frustum;
        private readonly DiscreteDynamicsWorld _world;
        private HashSet<CollisionObject> _insideSet;
        private Vector3[] _points;

        public BulletFrustum()
        {
            _bulletLock = new object();
            _bodiesLock = new object();
            _bodies = new Dictionary<ICullable, RigidBody>();
            _frustumShape = new FrustumShape();
            _callback = new FrustumContactTestCallback();
            var configuration = new DefaultCollisionConfiguration();
            var solver = new SequentialImpulseConstraintSolver();
            var broadphase = new DbvtBroadphase();
            var dispatcher = new CollisionDispatcher(configuration);
            _world = new DiscreteDynamicsWorld(dispatcher, broadphase, solver, configuration)
            {
                DebugDrawer = new BulletDraw
                {
                    DebugMode = DebugDrawModes.DrawAabb
                }
            };
            _frustum = CreateFrustumRigidbody();
            _world.AddRigidBody(_frustum);
        }

        private static RigidBody CreateFrustumRigidbody()
        {
            using (var bodyInfo = new RigidBodyConstructionInfo(1, new DefaultMotionState(), new BoxShape(BulletSharp.Math.Vector3.One)))
            {
                var rigid = new RigidBody(bodyInfo);
                rigid.CollisionFlags |= CollisionFlags.NoContactResponse;
                return rigid;
            }
        }
        
        private CollisionShape CreateFrustumShape(Matrix4 ModelView, Matrix4 Projection)
        {
            var inverseMvp = (ModelView * Projection).Inverted();
            var shape = new ConvexHullShape();
            _points = _unitCube.Select(V => Vector4.Transform(V, Projection.Inverted())).Select(P => (P.Xyz / P.W)).ToArray();
            for (var i = 0; i < _points.Length; ++i)
            {
                shape.AddPoint(_points[i].Compatible());
            }
            return shape;
        }

        private static void UpdateShape(RigidBody Body, CollisionShape NewShape)
        {
            var previous = Body.CollisionShape;
            try
            {
                Body.CollisionShape = NewShape;
            }
            finally
            {
                previous.Dispose();
            }
        }
        public void UpdateMatrices(Matrix4 ModelView, Matrix4 Projection)
        {
            UpdateShape(_frustum, CreateFrustumShape(ModelView, Projection));
            _frustum.WorldTransform = ModelView.Inverted().Compatible();
        }

        public void Update()
        {
            lock (_bodiesLock)
            {
                UpdateBoxes();
            }
            _callback.Reset();
            lock (_bulletLock)
            {
                _frustum.CollisionFlags |= CollisionFlags.CustomMaterialCallback;
                _world.PerformDiscreteCollisionDetection();
                _world.UpdateAabbs();
                _world.ContactTest(_frustum, _callback);
            }
            _insideSet = _callback.Objects;
        }

        public void Draw()
        {
            lock (_bulletLock)
            {
                for (var i = 0; i < _points.Length; ++i)
                {
                //    BasicGeometry.DrawPoint(_points[i] + _frustum.WorldTransform.Origin.Compatible(), Vector4.One);
                }

                //_world.DebugDrawObject(_frustum.WorldTransform, _frustum.CollisionShape, BulletSharp.Math.Vector3.One);
                _world.DebugDrawWorld();
            }
        }

        private void UpdateBoxes()
        {
            bool NeedsToBeInSimulation(ICullable CullableObject)
            {
                if (!CullableObject.Enabled) return false;
                if (!CullableObject.PrematureCulling) return true;
                return !((CullableObject.Position - GameManager.Player.Position).LengthSquared > GeneralSettings.DrawDistanceSquared);
            }

            foreach (var pair in _bodies)
            {
                var cullable = pair.Key;
                var body = pair.Value;
                var information = (CullingInformation) body.UserObject;
                
                if (NeedsToBeInSimulation(cullable))
                {
                    if(!information.IsInSimulation)
                        AddToSimulation(cullable, information);
                }
                else
                {
                    if(information.IsInSimulation)
                        RemoveFromSimulation(cullable, information);
                }

                if (information.IsInSimulation && information.NeedsUpdate(cullable))
                {
                    if (cullable is ChunkMesh)
                    {
                        int a = 0;
                    }
                    var size = cullable.Max - cullable.Min;
                    UpdateShape(body, new BoxShape(size.Compatible() * .5f));
                    body.WorldTransform = Matrix.Translation(cullable.Position.Compatible() + size.Compatible() * .5f);
                    
                    information.Max = cullable.Max;
                    information.Min = cullable.Min;
                    information.Position = cullable.Position;
                }
            }
        }
        
        public bool Contains(ICullable Cullable)
        {
            lock(_bodies)
                return _insideSet.Contains(_bodies[Cullable]);
        }

        private void AddToSimulation(ICullable Cullable, CullingInformation Information)
        {
            lock (_bulletLock) 
                _world.AddRigidBody(_bodies[Cullable]);
            Information.IsInSimulation = true;
        }

        private void RemoveFromSimulation(ICullable Cullable, CullingInformation Information)
        {
            lock (_bulletLock) 
                _world.RemoveRigidBody(_bodies[Cullable]);
            Information.IsInSimulation = false;
        }
        
        public void Add(ICullable Cullable)
        {
            lock(_bodiesLock)
                _bodies.Add(Cullable, CreateRigidbody(Cullable));
        }

        public void Remove(ICullable Cullable)
        {
            lock (_bodiesLock)
            {
                var information = (CullingInformation) _bodies[Cullable].UserObject;
                if(information.IsInSimulation)
                    RemoveFromSimulation(Cullable, information);
                DestroyRigidbody(_bodies[Cullable]);
                _bodies.Remove(Cullable);
            }
        }
        private static RigidBody CreateRigidbody(ICullable Cullable)
        {
            using (var bodyInfo = new RigidBodyConstructionInfo(0, new DefaultMotionState(), new BoxShape(BulletSharp.Math.Vector3.One)))
            {
                var rigid = new RigidBody(bodyInfo);
                rigid.CollisionFlags |= CollisionFlags.NoContactResponse;
                rigid.UserObject = new CullingInformation();
                return rigid;
            }
        }

        private static void DestroyRigidbody(RigidBody Body)
        {
            Body.CollisionShape.Dispose();
            Body.MotionState.Dispose();
            Body.Dispose();
        }

        private class CullingInformation
        {
            public Vector3 Position { get; set; }
            public Vector3 Min { get; set; }
            public Vector3 Max { get; set; }
            public bool IsInSimulation { get; set; }

            public bool NeedsUpdate(ICullable Cullable) =>
                Cullable.Max != Max || Cullable.Min != Min || Cullable.Position != Position;
        }
    }
}