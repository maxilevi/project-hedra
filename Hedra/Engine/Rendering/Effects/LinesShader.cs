/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 06/08/2016
 * Time: 12:41 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Rendering;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering.Effects
{
	/// <summary>
	/// Description of LinesShader.
	/// </summary>
	public class LinesShader : Shader
	{
		public int ColorUniform;
		
		public LinesShader(string s1, string s2) : base(s1,s2){}
		
		public override void GetUniformsLocations()
		{
			ColorUniform = GL.GetUniformLocation(ShaderID, "InColor");
		}
	}
}
