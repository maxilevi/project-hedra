/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/06/2016
 * Time: 04:18 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Rendering;

namespace Hedra.Engine.Rendering.UI
{
	/// <summary>
	/// Description of CursorShader.
	/// </summary>
	public class CursorShader : Shader
	{
		public int PositionUniform;
		public int ScaleUniform;
		
		public CursorShader(string S1, string S2) : base(S1,S2){}
		
		public override void GetUniformsLocations(){
			PositionUniform = GL.GetUniformLocation(ShaderID, "Position");
			ScaleUniform = GL.GetUniformLocation(ShaderID, "Scale");
		}
	}
}
