/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 01/07/2016
 * Time: 11:37 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Rendering;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Player
{
	/// <summary>
	/// Description of SkillsShader.
	/// </summary>
	public class SkillsShader : Shader
	{
		public int TintUniform;
		public int PositionUniform;
		public int ScaleUniform;
		public int BoolsUniform;
		public int CooldownUniform;
		public int MaskUniform;
		
		public SkillsShader(string s1, string s2) : base (s1,s2){}
		
		public override void GetUniformsLocations(){
			TintUniform = GL.GetUniformLocation(ShaderID, "Tint");
			ScaleUniform = GL.GetUniformLocation(ShaderID, "Scale");
			PositionUniform = GL.GetUniformLocation(ShaderID, "Position");
			BoolsUniform = GL.GetUniformLocation(ShaderID, "Bools");
			CooldownUniform = GL.GetUniformLocation(ShaderID, "Cooldown");
			MaskUniform = GL.GetUniformLocation(ShaderID, "Mask");	
		}
	}
}
