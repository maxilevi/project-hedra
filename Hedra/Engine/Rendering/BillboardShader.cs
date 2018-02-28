/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 12/06/2016
 * Time: 12:43 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of BillboardShader.
	/// </summary>
	public class BillboardShader : Shader
	{
		public int OffsetUniformLocation;
		public int ViewAlignUniformLocation;
		public int ScaleUniformLocation;
		public int OpacityUniformLocation;
		public int ColorUniformLocation;
	
		public BillboardShader(string s1, string s2) : base (s1,s2){}
		
		public override void GetUniformsLocations(){
			OffsetUniformLocation = GL.GetUniformLocation(ShaderID, "Offset");
			ViewAlignUniformLocation = GL.GetUniformLocation(ShaderID, "ViewAlign");
			ScaleUniformLocation = GL.GetUniformLocation(ShaderID, "Scale");
			OpacityUniformLocation = GL.GetUniformLocation(ShaderID, "Opacity");
			ColorUniformLocation = GL.GetUniformLocation(ShaderID, "UniformColor");
		}
	}
}
