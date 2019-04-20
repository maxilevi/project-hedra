/*
 * Author: Zaphyk
 * Date: 20/02/2016
 * Time: 08:34 p.m.
 *
 */

using Hedra.Engine.Rendering.Core;
using OpenTK;

namespace Hedra.Engine.Rendering
{
    public interface IMeshBuffer
    {
        VBO<Vector3> Vertices { get; }
        VBO<Vector4> Colors { get; }
        VBO<uint> Indices { get; }
        VBO<Vector3> Normals { get;  }
        VAO<Vector3, Vector4, Vector3> Data { get; }

        void Draw();

        void Dispose();
    }
}