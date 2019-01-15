using System.Drawing;
using System.Linq;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Frustum;
using Hedra.Engine.Rendering.Geometry;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.EnvironmentSystem
{
    public class SkyOverlay
    {
        private static readonly Bitmap DefaultBitmap;
        private static readonly Shader Shader;
        private readonly VBO<Vector3> _vertices;
        private readonly VAO<Vector3> _buffer;
        private readonly Cubemap _map;
        public Matrix4 TransformationMatrix { get; set; } = Matrix4.Identity;
        public Vector4 ColorMultiplier { get; set; } = Vector4.One;

        static SkyOverlay()
        {
            Shader = Shader.Build("Shaders/SkyOverlay.vert", "Shaders/SkyOverlay.frag");
        }

        public SkyOverlay(string[] Textures) : this()
        {
            _map = new Cubemap(Textures.Select( T => Graphics2D.LoadBitmapFromAssets(T ?? "Assets/Sky/empty.png")).ToArray());
        }

        private SkyOverlay()
        {
            var geometry = Geometry.Cube();
            _vertices = new VBO<Vector3>(geometry.Vertices, geometry.Vertices.Length * Vector3.SizeInBytes, VertexAttribPointerType.Float);
            _buffer = new VAO<Vector3>(_vertices);
        }

        public void Draw()
        {
            Renderer.Disable(EnableCap.DepthTest);
            Renderer.Enable(EnableCap.Blend);
            Shader.Bind();     
            _buffer.Bind();
            _map.Bind();
            
            Shader["map"] = 0;
            Shader["mvp"] = Culling.ModelViewMatrix.ClearTranslation() * Culling.ProjectionMatrix;
            Shader["trans_matrix"] = Matrix4.CreateScale(4) * TransformationMatrix;
            Shader["color_multiplier"] = ColorMultiplier;

            Renderer.DrawArrays(PrimitiveType.Triangles, 0, _vertices.Count);

            _map.Unbind();
            _buffer.Unbind();
            Shader.Unbind(); 
            Renderer.Disable(EnableCap.Blend);
            Renderer.Enable(EnableCap.DepthTest);
            Renderer.Enable(EnableCap.CullFace);
        }

        public void Dispose()
        {
            _vertices.Dispose();
            _buffer.Dispose();
        }
    }
}