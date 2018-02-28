/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 13/12/2016
 * Time: 07:14 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering.Particles
{
	/// <summary>
	/// Description of ParticleShader.
	/// </summary>
	public class ParticleShader : Shader
	{
		public int PlayerPositionUniform;
		
		public ParticleShader(string s1, string s2) : base(s1,s2){}
		
		public override void GetUniformsLocations()
		{
			PlayerPositionUniform = GL.GetUniformLocation(ShaderID, "PlayerPosition");
		}
	}
}
