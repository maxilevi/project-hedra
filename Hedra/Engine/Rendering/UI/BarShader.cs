/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 14/06/2016
 * Time: 07:54 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK.Graphics.OpenGL;
using OpenTK;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.Rendering.UI
{
	/// <summary>
	/// Description of BarShader.
	/// </summary>
	public class BarShader : Shader
	{
		public int ColorUniform;
		public int ScaleUniform;
		public int PositionUniform;
		public int DistanceUniform;
		public int TexUniform;
		public BarShader(string S1, string S2) : base(S1,S2){}
		
		public override void GetUniformsLocations(){
			ColorUniform = GL.GetUniformLocation(ShaderID, "Color");
			PositionUniform = GL.GetUniformLocation(ShaderID, "Position");
			ScaleUniform = GL.GetUniformLocation(ShaderID, "Scale");
			DistanceUniform = GL.GetUniformLocation(ShaderID, "TargetDistance");
			TexUniform = GL.GetUniformLocation(ShaderID, "BluePrint");
		}
	}
}
