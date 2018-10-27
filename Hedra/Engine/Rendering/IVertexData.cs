using System.Collections.Generic;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    public interface IVertexData
    {
        List<Vector3> Vertices { get; set; }
        List<Vector4> Colors { get; set; }
        List<Vector3> Normals { get; set; }
        List<uint> Indices { get; set; }
        List<float> Extradata { get; set; }
    }
}