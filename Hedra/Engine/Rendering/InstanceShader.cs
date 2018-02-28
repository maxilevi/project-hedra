/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 29/11/2016
 * Time: 06:11 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of InstanceShader.
	/// </summary>
	public class InstanceShader : Shader
	{
		public int WindUniform;
		public int HighestPointUniform;
		public int LowestPointUniform;
		public int TimeUniform;
		public int PlayerPositionUniform;
		public InstanceShader(string s1, string s2) : base(s1,s2){}
		
		public override void GetUniformsLocations(){
			WindUniform = GL.GetUniformLocation(ShaderID, "HasWind");
			LowestPointUniform = GL.GetUniformLocation(ShaderID, "LowestPoint"); 
			HighestPointUniform = GL.GetUniformLocation(ShaderID, "HighestPoint");
			TimeUniform = GL.GetUniformLocation(ShaderID, "Time");
			PlayerPositionUniform = GL.GetUniformLocation(ShaderID, "PlayerPosition");
		}
	}
}
