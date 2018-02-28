/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/09/2017
 * Time: 02:44 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of DropShadowShader.
	/// </summary>
	public class DropShadowShader : Shader
	{
		public int ScaleUniform;
		public int PositionUniform;
		public int PlayerPositionUniform;
		public int OpacityUniform;
		public int RotationUniform;

		public DropShadowShader(string s1, string s2) : base(s1,s2){}
		
		public override void GetUniformsLocations()
		{
			base.GetUniformsLocations();
			PlayerPositionUniform = GL.GetUniformLocation(ShaderID, "PlayerPosition");
			ScaleUniform = GL.GetUniformLocation(ShaderID, "Scale");
			PositionUniform = GL.GetUniformLocation(ShaderID, "Position");
			OpacityUniform = GL.GetUniformLocation(ShaderID, "Opacity");
			RotationUniform = GL.GetUniformLocation(ShaderID, "Rotation");
		}
	}
}
