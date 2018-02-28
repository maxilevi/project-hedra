using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace Hedra.Engine.Rendering.UI
{
	/// <summary>
	/// Description of GUIShader.
	/// </summary>
	 public class MinimapShader : Shader
	{
		public int ScaleUniform;
		public int SizeUniform;
		public int PositionUniform;
		public int ColorUniform;
		public int FillUniform;
		public int GuiUniform;
		public int FlippedUniform; 
		public int OpacityUniform;
		public int GrayscaleUniform, TintUniform;
		
		public MinimapShader(string S1, string S2) : base(S1, S2){}
		
		public override void GetUniformsLocations(){
			ScaleUniform = GL.GetUniformLocation(ShaderID, "Scale");
			PositionUniform = GL.GetUniformLocation(ShaderID, "Position");
			ColorUniform = GL.GetUniformLocation(ShaderID, "Color");
			GuiUniform = GL.GetUniformLocation(ShaderID, "Texture");
			FillUniform = GL.GetUniformLocation(ShaderID, "Fill");
			FlippedUniform = GL.GetUniformLocation(ShaderID, "Flipped");
			OpacityUniform = GL.GetUniformLocation(ShaderID, "Opacity");
			GrayscaleUniform = GL.GetUniformLocation(ShaderID, "Grayscale");
			TintUniform = GL.GetUniformLocation(ShaderID, "Tint");
			SizeUniform = GL.GetUniformLocation(ShaderID, "size");
		}
	}
}
