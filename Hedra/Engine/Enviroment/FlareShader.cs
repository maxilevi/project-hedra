/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 09/06/2017
 * Time: 06:44 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.Enviroment
{
	/// <summary>
	/// Description of FlareShader.
	/// </summary>
	public class FlareShader : Shader
	{
		public int BrightnessUniform, TransformUniform;
		public FlareShader(string s1, string s2) : base(s1,s2){}
		
		public override void GetUniformsLocations()
		{
			BrightnessUniform = GL.GetUniformLocation(ShaderID, "Brightness");
			TransformUniform = GL.GetUniformLocation(ShaderID, "Transform");
		}
		
	}
}
