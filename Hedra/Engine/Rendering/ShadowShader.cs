/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 29/06/2016
 * Time: 09:50 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of ShadowShader.
	/// </summary>
	public class ShadowShader : Shader
	{
		public int MVPUniform, TimeUniform, FancyUniform;
		public ShadowShader(string s1, string s2) : base(s1,s2){}
		
		public override void GetUniformsLocations(){
			MVPUniform = GL.GetUniformLocation(ShaderID, "MVP");
			FancyUniform = GL.GetUniformLocation(ShaderID, "Fancy");
			TimeUniform = GL.GetUniformLocation(ShaderID, "Time");
		}
	}
}
