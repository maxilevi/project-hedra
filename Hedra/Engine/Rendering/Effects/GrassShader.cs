/*
 * Author: Zaphyk
 * Date: 20/03/2016
 * Time: 12:20 p.m.
 *
 */
using System;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering.Effects
{
	/// <summary>
	/// Description of GrassShader.
	/// </summary>
	public class GrassShader : Shader
	{
		public int GrassMovementLocation;
		public int TopColorLocation;
		public int BotColorLocation;
		public int HeightLocation;
		public int MaxDistLocation;
		public int MinDistLocation;
		
		public GrassShader(string s1, string s2) : base(s1, s2){}
		public GrassShader(string s1, string s2, string s3) : base(s1, s2, s3){}
		
		public override void GetUniformsLocations(){
			GrassMovementLocation = GL.GetUniformLocation(ShaderID, "GrassMovement");
			HeightLocation = GL.GetUniformLocation(ShaderID, "U_Height");
			TopColorLocation = GL.GetUniformLocation(ShaderID, "U_TopColor");
			BotColorLocation = GL.GetUniformLocation(ShaderID, "U_BotColor");
			MaxDistLocation = GL.GetUniformLocation(ShaderID, "MaxDist");
			MinDistLocation = GL.GetUniformLocation(ShaderID, "MinDist");
		}
	}
}
