using System;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.PhysicsSystem
{
    public class ObjectMeshCollider : IDisposable
    {
        private readonly BoneBox _collider;
        public ObjectMesh Mesh { get; set; }

        public ObjectMeshCollider(ObjectMesh Mesh, VertexData Contents)
        {
            this.Mesh = Mesh;
            _collider = BoneBox.From(new BoneData
            {
                Id = 0,
                Vertices = Contents.Vertices.ToArray()
            });
        }

        private BoneBox TransformCollider(BoneBox Box)
        {
            for (var i = 0; i < Box.Corners.Length; i++)
            {
                Box.Corners[i] = Mesh.TransformPoint(Box.Corners[i]);
            }
            return Box;
        }

        public BoneBox Collider => this.TransformCollider(_collider);
        public CollisionShape Shape => Collider.ToShape();

        public void Dispose()
        {
            
        }
    }
}
