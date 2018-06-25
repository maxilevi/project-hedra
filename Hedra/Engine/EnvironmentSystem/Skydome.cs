using System;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.EnvironmentSystem
{
    public class Skydome
    {
        private static readonly Shader Shader;
        public uint OverlayTexture { get; }
        private VBO<Vector3> _vertices;
        private VBO<Vector3> _normals;
        private VBO<Vector2> _uvs;
        private VBO<ushort> _indices;
        private VAO<Vector3, Vector2> _buffer;

        static Skydome()
        {
            Shader = Shader.Build("Shaders/Skydome.vert", "Shaders/Skydome.frag");
        }

        public Skydome()
        {
            OverlayTexture = Graphics2D.LoadFromAssets("Assets/Sky/starry_night.png",
                TextureMinFilter.Nearest, TextureMagFilter.Nearest, TextureWrapMode.Repeat);   
        }

        public void Draw()
        {
            GraphicsLayer.Disable(EnableCap.DepthTest);
            GraphicsLayer.Disable(EnableCap.Blend);
            GraphicsLayer.Enable(EnableCap.Blend);
            Shader.Bind();     
            _buffer.Bind();
            
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, OverlayTexture);
            
            Shader["star_texture"] = 0;
            Shader["mvp"] = DrawManager.FrustumObject.ModelViewMatrix.ClearTranslation() * DrawManager.FrustumObject.ProjectionMatrix;
            Shader["trans_matrix"] = Matrix4.CreateScale(5f);
            Shader["uv_matrix"] = Matrix2.Identity;
            
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indices.ID);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedShort, IntPtr.Zero);
            
            _buffer.Unbind();
            Shader.UnBind(); 
            GraphicsLayer.Disable(EnableCap.Blend);
            GraphicsLayer.Enable(EnableCap.DepthTest);
            GraphicsLayer.Enable(EnableCap.CullFace);
        }
        
        private void Build()
        {
            var geometry = Geometry.UVSphere(1);
            this._uvs = new VBO<Vector2>(geometry.UVs, geometry.UVs.Length * Vector2.SizeInBytes, VertexAttribPointerType.Float);
            this._vertices = new VBO<Vector3>(geometry.Vertices, geometry.Vertices.Length * Vector3.SizeInBytes, VertexAttribPointerType.Float);
            this._indices = new VBO<ushort>(geometry.Indices, geometry.Indices.Length * sizeof(ushort), VertexAttribPointerType.UnsignedShort, BufferTarget.ElementArrayBuffer);
            this._buffer = new VAO<Vector3, Vector2>(_vertices, _uvs);
        }
    }
}