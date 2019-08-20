
using System;
using System.Drawing;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.Core;
using Hedra.Rendering;

namespace Hedra.Engine.Rendering
{
    /// <summary>
    /// Description of BasicGeometry.
    /// </summary>
    public static class BasicGeometry
    {
        private static readonly Line3D _line;
        private static readonly VBO<Vector3> _drawVerts;
        private static readonly VBO<uint> _drawIndices;
        private static readonly VAO<Vector3> _drawVao;

        private static VBO<Vector3> CubeVerticesVBO { get; }
        public static VBO<ushort> CubeIndicesVBO { get; }
        public static VAO<Vector3> CubeVAO { get; }

        static BasicGeometry()
        {
            var data = new CubeData();
            data.AddFace(Face.ALL);
            var asVertexData = new VertexData()
            {
                Vertices = data.VerticesArrays.ToList(),
                Indices = data.Indices,
                Normals = data.Normals.ToList(),
                Colors = Enumerable.Repeat(Vector4.One, data.VerticesArrays.Length).ToList()
            };
            asVertexData.Smooth();
            var indices = asVertexData.Indices.Select(I => (ushort) I).ToArray();
            CubeVerticesVBO = new VBO<Vector3>(asVertexData.Vertices.ToArray(), asVertexData.Vertices.Count * Vector3.SizeInBytes, VertexAttribPointerType.Float);
            CubeIndicesVBO = new VBO<ushort>(indices, indices.Length * sizeof(ushort), VertexAttribPointerType.UnsignedShort, BufferTarget.ElementArrayBuffer);
            CubeVAO = new VAO<Vector3>(CubeVerticesVBO);
            
            _drawVerts = new VBO<Vector3>(new Vector3[5], 5 * Vector3.SizeInBytes, VertexAttribPointerType.Float);
            _drawIndices = new VBO<uint>(new uint[5], 5 * sizeof(uint), VertexAttribPointerType.UnsignedInt, BufferTarget.ElementArrayBuffer);
            _drawVao = new VAO<Vector3>(_drawVerts);
            _line = new Line3D();
        }

        public static void DrawPoint(Vector3 Point, Vector4 Color)
        {
#if DEBUG
            Shader.Passthrough.Bind();
            OpenTK.Graphics.OpenGL.GL.PointSize(4);
            OpenTK.Graphics.OpenGL.GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.Points);
            OpenTK.Graphics.OpenGL.GL.Vertex3(Point);
            OpenTK.Graphics.OpenGL.GL.Color4(Color);
            OpenTK.Graphics.OpenGL.GL.End();
            Shader.Passthrough.Unbind();
#endif
        }

        public static void DrawLine(Vector3 Start, Vector3 End, Vector4 Color, float Width = 1)
        {
#if DEBUG
            Shader.Passthrough.Bind();
            OpenTK.Graphics.OpenGL.GL.LineWidth(Width);
            OpenTK.Graphics.OpenGL.GL.Begin(OpenTK.Graphics.OpenGL.PrimitiveType.Lines);
            OpenTK.Graphics.OpenGL.GL.Color4(Color);
            OpenTK.Graphics.OpenGL.GL.Vertex3(Start);
            OpenTK.Graphics.OpenGL.GL.Color4(Color);
            OpenTK.Graphics.OpenGL.GL.Vertex3(End);
            OpenTK.Graphics.OpenGL.GL.End();
            Shader.Passthrough.Unbind();
#endif
        }

        public static void DrawShapes(CollisionShape[] Shapes, Vector4 DrawColor)
        {
            for (var i = 0; i < Shapes.Length; i++)
            {
                DrawShape(Shapes[i], DrawColor);
            }
        }

        public static void DrawShape(CollisionShape Shape, Vector4 DrawColor)
        {
            Shader.Passthrough.Bind();
            Renderer.Disable(EnableCap.CullFace);
            Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            _drawVerts.Update(Shape.Vertices, Shape.Vertices.Length);
            _drawIndices.Update(Shape.Indices, Shape.Indices.Length);

            _drawVao.Bind();
            _drawIndices.Bind();

            //Renderer.DrawArrays(PrimitiveType.Triangles, 0, _drawVerts.Count);
            Renderer.DrawElements(PrimitiveType.Triangles, _drawIndices.Count, DrawElementsType.UnsignedInt, IntPtr.Zero);
            
            _drawIndices.Unbind();
            _drawVao.Unbind();
            
            Renderer.Enable(EnableCap.CullFace);
            Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            Shader.Passthrough.Unbind();
        }
    }
}
