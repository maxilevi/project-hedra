using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;

namespace Hedra.Engine.Rendering.UI
{
	/// <summary>
	/// Description of GUIShader.
	/// </summary>
	 public class GUIShader : Shader, FXShader
    {
		public int ScaleUniform { get; set; }
        public int PositionUniform { get; set; }
        public int ColorUniform;
        public int BackGroundUniform { get; set; }
        public int GUIUniform { get; set; }
        public int FlippedUniform { get; set; }
        public int SizeUniform { get; set; }
        public int FxaaUniform { get; set; }
        public int OpacityUniform;
        public int GrayscaleUniform, TintUniform, RotationUniform;
		
		public GUIShader(string S1, string S2) : base(S1, S2){}
		
		public override void GetUniformsLocations(){
			ScaleUniform = GL.GetUniformLocation(ShaderID, "Scale");
			PositionUniform = GL.GetUniformLocation(ShaderID, "Position");
			ColorUniform = GL.GetUniformLocation(ShaderID, "Color");
			GUIUniform = GL.GetUniformLocation(ShaderID, "Texture");
			BackGroundUniform = GL.GetUniformLocation(ShaderID, "Background");
			FlippedUniform = GL.GetUniformLocation(ShaderID, "Flipped");
			OpacityUniform = GL.GetUniformLocation(ShaderID, "Opacity");
			GrayscaleUniform = GL.GetUniformLocation(ShaderID, "Grayscale");
			TintUniform = GL.GetUniformLocation(ShaderID, "Tint");
			RotationUniform = GL.GetUniformLocation(ShaderID, "Rotation");
            SizeUniform = GL.GetUniformLocation(ShaderID, "Size");
            FxaaUniform = GL.GetUniformLocation(ShaderID, "FXAA");
        }
	}
}
