
using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.Rendering
{
    /// <summary>
    /// Description of BasicGeometry.
    /// </summary>
    public static class BasicGeometry
    {
        private static VBO<Vector3> _cubeVerts;    
        private static VBO<uint> _cubeIndices;
        private static readonly VBO<Vector3> _drawVerts;
        private static readonly VBO<uint> _drawIndices;
        private static readonly VAO<Vector3> _drawVao;
        
        static BasicGeometry()
        {
            var data = new CubeData();
            data.AddFace(Face.ALL);
            _cubeVerts = new VBO<Vector3>(data.VerticesArrays, data.VerticesArrays.Length * Vector3.SizeInBytes, VertexAttribPointerType.Float);
            _cubeIndices = new VBO<uint>(data.Indices.ToArray(), data.Indices.Count * sizeof(uint), VertexAttribPointerType.UnsignedInt, BufferTarget.ElementArrayBuffer);
            _drawVerts = new VBO<Vector3>(new Vector3[5], 5 * Vector3.SizeInBytes, VertexAttribPointerType.Float);
            _drawIndices = new VBO<uint>(new uint[5], 5 * sizeof(uint), VertexAttribPointerType.UnsignedInt, BufferTarget.ElementArrayBuffer);
            _drawVao = new VAO<Vector3>(_drawVerts);
        }

        public static void DrawBox(Vector3 Start, Vector3 End)
        {
            //TODO:
        }

        public static void DrawLine(Vector3 Start, Vector3 End, Vector4 Color)
        {
            //TODO:
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
