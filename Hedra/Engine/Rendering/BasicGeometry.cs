
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
        
        static BasicGeometry()
        {
            var data = new CubeData();
            data.AddFace(Face.ALL);
            _cubeVerts = new VBO<Vector3>(data.VerticesArrays, data.VerticesArrays.Length * Vector3.SizeInBytes, VertexAttribPointerType.Float);
            _cubeIndices = new VBO<uint>(data.Indices.ToArray(), data.Indices.Count * sizeof(uint), VertexAttribPointerType.UnsignedInt, BufferTarget.ElementArrayBuffer);
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
            Renderer.Disable(EnableCap.CullFace);
            Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);

            //TODO:
            
            Renderer.Enable(EnableCap.CullFace);
            Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }
    }
}
