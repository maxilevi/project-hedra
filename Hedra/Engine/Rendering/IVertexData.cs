using System.Collections.Generic;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    public interface IVertexData
    {
        IList<Vector3> Vertices { get; set; }
        IList<Vector4> Colors { get; set; }
        IList<Vector3> Normals { get; set; }
        IList<uint> Indices { get; set; }
    }
}