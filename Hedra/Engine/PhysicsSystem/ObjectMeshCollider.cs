using System;
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.IO;
using Hedra.Engine.Rendering;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.PhysicsSystem
{
    public class ObjectMeshCollider : IDisposable
    {
        private static readonly Dictionary<VertexData, BoneBox> Cache = new Dictionary<VertexData, BoneBox>();
        private readonly BoneBox _originalCollider;
        private readonly BoneBox _modifiedCollider;
        public ObjectMesh Mesh { get; }

        public ObjectMeshCollider(ObjectMesh Mesh, VertexData Contents)
        {
            this.Mesh = Mesh;
            /*var key = Contents?.Original;
            if (key != null && Cache.ContainsKey(key) || !Cache.ContainsKey(Contents))
            {
                Cache.Add(key ?? Contents, BoneBox.From(new BoneData
                {
                    Id = 0,
                    Vertices = Contents.Vertices.ToArray()
                }));
                Log.WriteLine($"[CACHE] Registered a new object collider cache. (Total = {Cache.Keys.Count})");
            }*/
            _originalCollider = BoneBox.From(new BoneData
            {
                Id = 0,
                Vertices = Contents.Vertices.ToArray()
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

        public BoneBox Collider => TransformCollider();
        public CollisionShape Shape => Collider.ToShape();

        public void Dispose()
        {
            
        }
    }
}
