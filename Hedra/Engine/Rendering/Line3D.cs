using System.Linq;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Core;
using Hedra.Game;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Rendering
{
    public class Line3D : IRenderable
    {
        private static readonly Shader LineShader = Shader.Build("Shaders/Lines3D.vert", "Shaders/Lines3D.frag");
        private readonly VAO<Vector3, Vector4> _data;
        private readonly VBO<Vector3> _vertices;
        private readonly VBO<Vector4> _colors;
        private Vector3[] _pointsArray;
        private Vector4[] _colorsArray;
        private bool _enabled;
        
        public Line3D()
        {
            _vertices = new VBO<Vector3>(new Vector3[0], 0, VertexAttribPointerType.Float);
            _colors = new VBO<Vector4>(new Vector4[0], 0, VertexAttribPointerType.Float);
            _data = new VAO<Vector3, Vector4>(_vertices, _colors);
            DrawManager.TrailRenderer.Add(this);
        }

        public void Update(Vector3[] Points, Vector4[] Colors)
        {
            if(_pointsArray != null && Points.SequenceEqual(_pointsArray) && _colorsArray != null && Colors.SequenceEqual(_colorsArray)) return;
            _vertices.Update(_pointsArray = Points, Points.Length * Vector3.SizeInBytes);
            _colors.Update(_colorsArray = Colors, Colors.Length * Vector4.SizeInBytes);
        }
        
        public void Draw()
        {
            if(!Enabled) return;
            LineShader.Bind();
            Renderer.LineWidth(Width);

            _data.Bind();
            LineShader["PlayerPosition"] = GameManager.Player.Position;
            Renderer.DrawArrays(PrimitiveType.LineStrip, 0, _vertices.Count);   
            _data.Unbind();

            Renderer.LineWidth(1);
            LineShader.Unbind();
        }

        public float Width { get; set; } = 5;
        
        public bool Enabled { get; set; }

        public void Dispose()
        {
            _data.Dispose();
            _vertices.Dispose();
            _colors.Dispose();
            DrawManager.UIRenderer.Remove(this);
        }
    }
}