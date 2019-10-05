using Hedra.Engine.Core;
using Hedra.Rendering;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    public struct NativeVertexData
    {
        public NativeList<Vector3> Vertices { get; set; }
        public NativeList<Vector4> Colors { get; set; }
        public NativeList<Vector3> Normals { get; set; }
        public NativeList<uint> Indices { get; set; }
        public NativeList<float> Extradata { get; set; }
        
        public NativeVertexData(IAllocator Allocator)
        {
            Vertices = new NativeList<Vector3>(Allocator);
            Colors = new NativeList<Vector4>(Allocator);
            Normals = new NativeList<Vector3>(Allocator);
            Indices = new NativeList<uint>(Allocator);
            Extradata = new NativeList<float>(Allocator);
        }
    }
}