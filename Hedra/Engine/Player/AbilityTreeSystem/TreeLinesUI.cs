using System.Numerics;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Windowing;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    public class TreeLinesUI : IRenderable, UIElement
    {
        private static readonly Shader LineShader = Shader.Build("Shaders/Lines.vert", "Shaders/Lines.frag");
        private readonly VBO<Vector4> _colors;
        private readonly VAO<Vector2, Vector4> _data;
        private readonly VBO<Vector2> _vertices;
        private bool _enabled;

        public TreeLinesUI()
        {
            _vertices = new VBO<Vector2>(new Vector2[0], 0, VertexAttribPointerType.Float);
            _colors = new VBO<Vector4>(new Vector4[0], 0, VertexAttribPointerType.Float);
            _data = new VAO<Vector2, Vector4>(_vertices, _colors);
            DrawManager.UIRenderer.Add(this);
        }

        public float Width { get; set; } = 5;

        public void Draw()
        {
            if (!_enabled) return;
            LineShader.Bind();
            Renderer.LineWidth(Width);

            _data.Bind();
            Renderer.DrawArrays(PrimitiveType.Lines, 0, _vertices.Count);
            _data.Unbind();

            Renderer.LineWidth(1);
            LineShader.Unbind();
        }

        public Vector2 Position { get; set; }

        public Vector2 Scale { get; set; }

        public void Enable()
        {
            _enabled = true;
        }

        public void Disable()
        {
            _enabled = false;
        }

        public void Dispose()
        {
            _data.Dispose();
            _vertices.Dispose();
            _colors.Dispose();
            DrawManager.UIRenderer.Remove(this);
        }

        public void Update(Vector2[] Lines, Vector4[] Colors)
        {
            _vertices.Update(Lines, Lines.Length * HedraSize.Vector2);
            _colors.Update(Colors, Colors.Length * HedraSize.Vector4);
        }
    }
}