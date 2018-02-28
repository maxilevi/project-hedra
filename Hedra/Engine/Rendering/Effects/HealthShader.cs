/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 05/06/2016
 * Time: 11:51 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering.Effects
{
	/// <summary>
	/// Description of HealthShader.
	/// </summary>
	public class HealthShader : Shader
	{
		public int OffsetUniformLocation;
		public int ModelViewUniformLocation;
		public int ScaleUniformLocation;
		public int DistanceUniformLocation;
		public int GradientStartUniformLocation;
		public int GradientEndUniformLocation;
		public int TransMatrixUniformLocation;
		public int GradientValueUniformLocation;
		
		public HealthShader(string s1, string s2) : base(s1,s2){}
		
		public override void GetUniformsLocations(){
			OffsetUniformLocation = GL.GetUniformLocation(ShaderID, "Offset");
			ModelViewUniformLocation = GL.GetUniformLocation(ShaderID, "MV");
			ScaleUniformLocation = GL.GetUniformLocation(ShaderID, "Scale");
			DistanceUniformLocation = GL.GetUniformLocation(ShaderID, "TargetDistance");
			GradientStartUniformLocation = GL.GetUniformLocation(ShaderID, "GradientStart");
			GradientEndUniformLocation = GL.GetUniformLocation(ShaderID, "GradientEnd");
			TransMatrixUniformLocation = GL.GetUniformLocation(ShaderID, "TransMatrix");
			GradientValueUniformLocation = GL.GetUniformLocation(ShaderID, "Factor");
		}
	}
}
