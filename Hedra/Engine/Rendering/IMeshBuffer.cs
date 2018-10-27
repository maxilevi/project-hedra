/*
 * Author: Zaphyk
 * Date: 20/02/2016
 * Time: 08:34 p.m.
 *
 */
using OpenTK;

namespace Hedra.Engine.Rendering
{
    public interface IMeshBuffer
    {
        VBO<Vector3> Vertices { get; set; }
        VBO<Vector4> Colors { get; set; }
        VBO<uint> Indices { get; set; }
        VBO<Vector3> Normals { get; set; }
        VAO<Vector3, Vector4, Vector3> Data { get; set; }

        void Draw();

        void Dispose();
    }
}