using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Windowing;
using Hedra.Game;

namespace Hedra.Engine.Rendering
{
    public class Line3D : IRenderable
    {
        private static Shader LineShader;
        private VBO<Vector4> _colors;
        private Vector4[] _colorsArray;
        private VAO<Vector3, Vector4> _data;
        private bool _disposed;
        private bool _enabled;
        private Vector3[] _pointsArray;
        private VBO<Vector3> _vertices;
        private bool _wasEnabled;

        static Line3D()
        {
            Executer.ExecuteOnMainThread(() =>
            {
                LineShader = Shader.Build("Shaders/Lines3D.vert", "Shaders/Lines3D.frag");
            });
        }

        public Line3D()
        {
            Executer.ExecuteOnMainThread(() =>
            {
                _vertices = new VBO<Vector3>(new Vector3[0], 0, VertexAttribPointerType.Float);
                _colors = new VBO<Vector4>(new Vector4[0], 0, VertexAttribPointerType.Float);
                _data = new VAO<Vector3, Vector4>(_vertices, _colors);
            });
            DrawManager.TrailRenderer.Add(this);
        }

        public float Width { get; set; } = 5;

        public bool Enabled { get; set; }

        public void Draw()
        {
            if (!Enabled || _data == null || _disposed) return;
            LineShader.Bind();
            Renderer.LineWidth(Width);

            _data.Bind();
            LineShader["PlayerPosition"] = GameManager.Player.Position;
            Renderer.DrawArrays(PrimitiveType.LineStrip, 0, _vertices.Count);
            _data.Unbind();

            Renderer.LineWidth(1);
            LineShader.Unbind();
        }

        public void Update(Vector3[] Points, Vector4[] Colors)
        {
            if (PositionSequenceEqual(Points, _pointsArray) && ColorSequenceEqual(Colors, _colorsArray)) return;
            Executer.ExecuteOnMainThread(() =>
            {
                if (_disposed) return;
                _vertices.Update(Points, Points.Length * HedraSize.Vector3);
                _colors.Update(Colors, Colors.Length * HedraSize.Vector4);
            });
            _pointsArray = Points;
            _colorsArray = Colors;
        }

        private static bool ColorSequenceEqual(IList<Vector4> A, IList<Vector4> B)
        {
            if (A == null || B == null || A.Count != B.Count) return false;
            for (var i = 0; i < A.Count; ++i)
            {
                if (Math.Abs(A[i].X - B[i].X) > 0.05f) return false;
                if (Math.Abs(A[i].Y - B[i].Y) > 0.05f) return false;
                if (Math.Abs(A[i].Z - B[i].Z) > 0.05f) return false;
            }

            return true;
        }

        private static bool PositionSequenceEqual(ICollection<Vector3> A, ICollection<Vector3> B)
        {
            if (A == null || B == null || A.Count != B.Count) return false;
            return ColorSequenceEqual(A.Select(V => new Vector4(V, 1)).ToArray(),
                B.Select(V => new Vector4(V, 1)).ToArray());
        }

        public void Dispose()
        {
            _disposed = true;

            void Dispose()
            {
                _data.Dispose();
                _vertices.Dispose();
                _colors.Dispose();
            }

            if (_data == null)
                Executer.ExecuteOnMainThread(Dispose);
            else
                Dispose();
            DrawManager.UIRenderer.Remove(this);
        }
    }
}