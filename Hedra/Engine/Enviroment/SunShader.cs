/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 03/12/2016
 * Time: 07:46 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.Enviroment
{
	/// <summary>
	/// Description of SunShader.
	/// </summary>
	public class SunShader : Shader
	{
		public int TextureUniform, DirectionUniform, TransMatrixUniform, PositionUniform;
		
		public SunShader(string s1, string s2) : base(s1,s2) {}
		
		public override void GetUniformsLocations()
		{
			TextureUniform = GL.GetUniformLocation(ShaderID, "Texture");
			PositionUniform = GL.GetUniformLocation(ShaderID, "Position");
			DirectionUniform = GL.GetUniformLocation(ShaderID, "Direction");
			TransMatrixUniform = GL.GetUniformLocation(ShaderID, "TransMatrix");
		}
	}
}