using System;
using System.Linq;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.PhysicsSystem
{
    public class ObjectMeshCollider : IDisposable
    {
        private readonly BoneBox _originalCollider;
        private readonly BoneBox _modifiedCollider;
        public ObjectMesh Mesh { get; set; }

        public ObjectMeshCollider(VertexData Contents) : this(null, Contents)
        {
        }

        public ObjectMeshCollider(ObjectMesh Mesh, VertexData Contents)
        {
            this.Mesh = Mesh;            
            _originalCollider = BoneBox.From(new BoneData
            {
                Id = 0,
                Vertices = Contents?.Vertices.ToArray() ?? new Vector3[8] 
            });
            _modifiedCollider = new BoneBox(0, _originalCollider.Corners.ToArray());
        }

        private BoneBox TransformCollider()
        {
            for (var i = 0; i < _modifiedCollider.Corners.Length; i++)
            {
                _modifiedCollider.Corners[i] = Mesh.TransformPoint(_originalCollider.Corners[i]);
            }
            return _modifiedCollider;
        }

        public BoneBox Collider => this.TransformCollider();
        public CollisionShape Shape => Collider.ToShape();

        public void Dispose()
        {
            
        }
    }
}
