/*
 * Author: Zaphyk
 * Date: 27/02/2016
 * Time: 05:30 a.m.
 *
 */
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;
using System.Drawing;
using Hedra.Engine.Rendering.UI;

namespace Hedra.Engine.EnvironmentSystem
{
	/// <summary>
	/// Description of Skydome.
	/// </summary>
	public sealed class Skydome
	{
	    private static readonly Shader SkyGradientShader;
	    private static readonly Shader SkyStarsShader;
	    private static readonly uint OverlayTexture;
        private int _previousShader;
	    private VBO<Vector3> _vertices;
	    private VBO<Vector3> _normals;
	    private VBO<Vector2> _uvs;
	    private VBO<ushort> _indices;
	    private VAO<Vector3, Vector2> _buffer;
	    public bool Enabled { get; set; } = true;
        public Vector4 TopColor { get; set; } = Color.CornflowerBlue.ToVector4();
        public Vector4 BotColor { get; set; } = Color.LightYellow.ToVector4();

        static Skydome()
        {
            OverlayTexture = Graphics2D.LoadFromAssets("Assets/Sky/starry_night.png", TextureMinFilter.Nearest, TextureMagFilter.Nearest, TextureWrapMode.Repeat);
            SkyGradientShader = Shader.Build("Shaders/SkyGradient.vert", "Shaders/SkyGradient.frag");
	        SkyStarsShader = Shader.Build("Shaders/SkyStars.vert", "Shaders/SkyStars.frag");
        }

        public Skydome()
        {
			this.Build();
        }
		
		public void Draw(){
			if(!Enabled) return;

            GraphicsLayer.Disable(EnableCap.DepthTest);
			GraphicsLayer.Disable(EnableCap.Blend);
			_previousShader = GraphicsLayer.ShaderBound;

			SkyGradientShader.Bind();
		    SkyGradientShader["topColor"] = TopColor;
			SkyGradientShader["botColor"] = BotColor;
			SkyGradientShader["height"] = (float) GameSettings.Height;
		    DrawManager.UIRenderer.SetupQuad();
		    DrawManager.UIRenderer.DrawQuad();

		    GraphicsLayer.Enable(EnableCap.Blend);
            SkyStarsShader.Bind();     
            _buffer.Bind();
		    SkyStarsShader["mvp"] = DrawManager.FrustumObject.ModelViewMatrix.ClearTranslation() * DrawManager.FrustumObject.ProjectionMatrix;
		    SkyStarsShader["trans_matrix"] = Matrix4.CreateScale(5f);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, OverlayTexture);
		    SkyStarsShader["star_texture"] = 0;
		    GL.BindBuffer(BufferTarget.ElementArrayBuffer, _indices.ID);
            GL.DrawElements(PrimitiveType.Triangles, _indices.Count, DrawElementsType.UnsignedShort, IntPtr.Zero);
            _buffer.Unbind();

            GL.UseProgram(_previousShader);
			GraphicsLayer.ShaderBound = _previousShader;
		    GraphicsLayer.Disable(EnableCap.Blend);
            GraphicsLayer.Enable(EnableCap.DepthTest);
			GraphicsLayer.Enable(EnableCap.CullFace);
		}
		
		private void Build()
		{
		    var geometry = Geometry.Skydome(12);
		    this._uvs = new VBO<Vector2>(geometry.UVs, geometry.UVs.Length * Vector2.SizeInBytes, VertexAttribPointerType.Float);
            this._vertices = new VBO<Vector3>(geometry.Vertices, geometry.Vertices.Length * Vector3.SizeInBytes, VertexAttribPointerType.Float);
		    this._indices = new VBO<ushort>(geometry.Indices, geometry.Indices.Length * sizeof(ushort), VertexAttribPointerType.UnsignedShort, BufferTarget.ElementArrayBuffer);
		    this._buffer = new VAO<Vector3, Vector2>(_vertices, _uvs);
		}
	}
}