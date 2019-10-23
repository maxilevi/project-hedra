using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Rendering.Animation;
using System.Numerics;

namespace Hedra.Engine.PhysicsSystem
{
    public class AnimatedCollider : IDisposable
    {
        public AnimatedModel Model { get; }
        private readonly AnimatedColliderData _colliderData;
        private readonly CollisionShape[] _shapes;
        private CollisionShape _defaultBroadphaseShape;
        private CollisionShape _horizontalBroadphaseShape;

        public AnimatedCollider(string Identifier, AnimatedModel Model)
        {
            this.Model = Model;
            this._colliderData = AnimatedColliderBuilder.Build(Identifier, Model);
            this._shapes = new CollisionShape[_colliderData.BonesData.Length];
        }

        public CollisionShape HorizontalBroadphase
        {
            get
            {
                var transforms = Model.JointTransforms;
                if (_horizontalBroadphaseShape == null)
                    _horizontalBroadphaseShape = new CollisionShape(new Vector3[_colliderData.DefaultBroadphase.Length]);
                for (var i = 0; i < _horizontalBroadphaseShape.Vertices.Length; i++)
                {
                    _horizontalBroadphaseShape.Vertices[i] = Vector3.Transform(
                        _colliderData.DefaultBroadphase[i].Vertex,
                        transforms[(int)_colliderData.DefaultBroadphase[i].Id.X]
                    );
                }
                _horizontalBroadphaseShape.RecalculateBroadphase(new Vector3(1, 0, 1));
                return _horizontalBroadphaseShape;
            }
        }
        
        public CollisionShape Broadphase
        {
            get
            {
                var transforms = Model.JointTransforms;
                if (_defaultBroadphaseShape == null)
                    _defaultBroadphaseShape = new CollisionShape(new Vector3[_colliderData.DefaultBroadphase.Length]);
                for (var i = 0; i < _defaultBroadphaseShape.Vertices.Length; i++)
                {
                    _defaultBroadphaseShape.Vertices[i] = Vector3.Transform(
                        _colliderData.DefaultBroadphase[i].Vertex,
                        transforms[(int)_colliderData.DefaultBroadphase[i].Id.X]
                    );
                }
                _defaultBroadphaseShape.RecalculateBroadphase();
                return _defaultBroadphaseShape;
            }
        }

        public BoneBox[] Colliders
        {
            get
            {
                var boneBoxes = _colliderData.DefaultBoneBoxes.Select(B => B.Clone()).ToArray();
                var transforms = Model.JointTransforms;
                for (var i = 0; i < boneBoxes.Length; i++)
                {
                    boneBoxes[i].Transform(transforms[boneBoxes[i].JointId]);
                }
                return boneBoxes;
            }
        }

        public Vector3[] Vertices
        {
            get
            {
                var colliders = this.Colliders;
                var vertexList = new List<Vector3>();
                for (var i = 0; i < colliders.Length; i++)
                {
                    vertexList.AddRange(colliders[i].Corners);
                }
                return vertexList.ToArray();
            }
        }

        public CollisionShape[] Shapes
        {
            get
            {
                var colliders = this.Colliders;
                for (var i = 0; i < colliders.Length; i++)
                {
                    _shapes[i] = colliders[i].ToShape();
                }
                return _shapes;
            }
        }

        public void Dispose()
        {

        }
    }
}
