using System;
using System.Linq;
using System.Numerics;
using Hedra.Engine.Bullet;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Windowing;
using Hedra.Rendering;
#if DEBUG
using Legacy = Silk.NET.OpenGL.Legacy;
#endif

namespace Hedra.Engine.Rendering
{
    /// <summary>
    ///     Description of BasicGeometry.
    /// </summary>
    public static class BasicGeometry
    {
        private static readonly Line3D _line;
        private static readonly VBO<Vector3> _drawVerts;
        private static readonly VBO<uint> _drawIndices;
        private static readonly VAO<Vector3> _drawVao;
        private static BulletDraw _debugDraw;
#if DEBUG
        private static readonly Legacy.GL _gl;
#endif

        static BasicGeometry()
        {
            var data = new CubeData();
            data.AddFace(Face.ALL);
            var asVertexData = new VertexData
            {
                Vertices = data.VerticesArrays.ToList(),
                Indices = data.Indices,
                Normals = data.Normals.ToList(),
                Colors = Enumerable.Repeat(Vector4.One, data.VerticesArrays.Length).ToList()
            };
            asVertexData.UniqueVertices();
            var indices = asVertexData.Indices.Select(I => (ushort)I).ToArray();
            CubeVerticesVBO = new VBO<Vector3>(asVertexData.Vertices.ToArray(),
                asVertexData.Vertices.Count * HedraSize.Vector3, VertexAttribPointerType.Float);
            CubeIndicesVBO = new VBO<ushort>(indices, indices.Length * sizeof(ushort),
                VertexAttribPointerType.UnsignedShort, BufferTarget.ElementArrayBuffer);
            CubeVAO = new VAO<Vector3>(CubeVerticesVBO);

            _drawVerts = new VBO<Vector3>(new Vector3[5], 5 * HedraSize.Vector3, VertexAttribPointerType.Float);
            _drawIndices = new VBO<uint>(new uint[5], 5 * sizeof(uint), VertexAttribPointerType.UnsignedInt,
                BufferTarget.ElementArrayBuffer);
            _drawVao = new VAO<Vector3>(_drawVerts);
            _line = new Line3D();
            _debugDraw = new BulletDraw();
#if DEBUG
            _gl = Legacy.GL.GetApi(Program.GameWindow.Window);
#endif
        }

        private static VBO<Vector3> CubeVerticesVBO { get; }
        public static VBO<ushort> CubeIndicesVBO { get; }
        public static VAO<Vector3> CubeVAO { get; }

        public static void DrawPlane(Vector3 Normal, float PlaneConst, Vector3 Position, Vector4 Color)
        {
            /*var mat = Transform.Compatible();
            var color = Color.Xyz().Compatible();
            var normal = Normal.Compatible();
            _debugDraw.DrawPlane(ref normal, PlaneConst, ref mat, ref color);*/
        }

        public static void DrawBox(Vector3 Min, Vector3 Max, Vector4 Color)
        {
            var p2 = new Vector3(Max.X, Min.Y, Min.Z);
            var p3 = new Vector3(Max.X, Max.Y, Min.Z);
            var p4 = new Vector3(Min.X, Max.Y, Min.Z);
            var p5 = new Vector3(Min.X, Min.Y, Max.Z);
            var p6 = new Vector3(Max.X, Min.Y, Max.Z);
            var p8 = new Vector3(Min.X, Max.Y, Max.Z);

            DrawLine(Min, p2, Color);
            DrawLine(p2, p3, Color);
            DrawLine(p3, p4, Color);
            DrawLine(p4, Min, Color);

            DrawLine(Min, p5, Color);
            DrawLine(p2, p6, Color);
            DrawLine(p3, Max, Color);
            DrawLine(p4, p8, Color);

            DrawLine(p5, p6, Color);
            DrawLine(p6, Max, Color);
            DrawLine(Max, p8, Color);
            DrawLine(p8, p5, Color);
        }

        public static void DrawPoint(Vector3 Point, Vector4 Color, float Width = 4)
        {
#if DEBUG

            Shader.Passthrough.Bind();
            _gl.PointSize(Width);
            _gl.Begin(Legacy.GLEnum.Points);
            _gl.Vertex3(Point.X, Point.Y, Point.Z);
            _gl.Color4(Color.X, Color.Y, Color.Z, Color.W);
            _gl.End();
            Shader.Passthrough.Unbind();
#endif
        }

        public static void DrawLine(Vector3 Start, Vector3 End, Vector4 Color, float Width = 1)
        {
#if DEBUG

            Shader.Passthrough.Bind();
            _gl.LineWidth(Width);
            _gl.Begin(Legacy.GLEnum.Lines);
            _gl.Color4(Color.X, Color.Y, Color.Z, Color.W);
            _gl.Vertex3(Start.X, Start.Y, Start.Z);
            _gl.Color4(Color.X, Color.Y, Color.Z, Color.W);
            _gl.Vertex3(End.X, End.Y, End.Z);
            _gl.End();
            Shader.Passthrough.Unbind();
#endif
        }

        public static void DrawShapes(CollisionShape[] Shapes, Vector4 DrawColor)
        {
            for (var i = 0; i < Shapes.Length; i++) DrawShape(Shapes[i], DrawColor);
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
            Renderer.DrawElements(PrimitiveType.Triangles, _drawIndices.Count, DrawElementsType.UnsignedInt,
                IntPtr.Zero);

            _drawIndices.Unbind();
            _drawVao.Unbind();

            Renderer.Enable(EnableCap.CullFace);
            Renderer.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            Shader.Passthrough.Unbind();
        }
    }
}