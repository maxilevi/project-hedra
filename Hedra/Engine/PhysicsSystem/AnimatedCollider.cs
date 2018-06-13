using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.Rendering.Animation;
using OpenTK;

namespace Hedra.Engine.PhysicsSystem
{
    public class AnimatedCollider : IDisposable
    {
        public AnimatedModel Model { get; }
        private readonly AnimatedColliderData _colliderData;
        private readonly CollisionShape[] _shapes;
        private CollisionShape _defaultBroadphaseShape;

        public AnimatedCollider(string Identifier, AnimatedModel Model)
        {
            this.Model = Model;
            this._colliderData = AnimatedColliderBuilder.Build(Identifier, Model);
            this._shapes = new CollisionShape[_colliderData.BonesData.Length];
        }

        public CollisionShape Broadphase
        {
            get
            {
                if (_defaultBroadphaseShape == null)
                    _defaultBroadphaseShape = new CollisionShape(new Vector3[_colliderData.DefaultBroadphase.Length], BoneBox.Indices);
                for (var i = 0; i < _defaultBroadphaseShape.Vertices.Length; i++)
                {
                    _defaultBroadphaseShape.Vertices[i] = new Vector3( 
                        Vector3.TransformPosition(_colliderData.DefaultBroadphase[i].Vertex,
                        Model.JointTransforms[(int)_colliderData.DefaultBroadphase[i].Id.X]).X, 
                        Vector3.TransformPosition(_colliderData.DefaultBroadphase[i].Vertex,
                        Model.JointTransforms[(int)_colliderData.DefaultBroadphase[i].Id.Y]).Y,
                        Vector3.TransformPosition(_colliderData.DefaultBroadphase[i].Vertex,
                        Model.JointTransforms[(int)_colliderData.DefaultBroadphase[i].Id.Z]).Z
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
