/*
 * Author: Zaphyk
 * Date: 21/02/2016
 * Time: 10:31 p.m.
 *
 */
using System;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.Effects
{
	public class BlurShader : Shader
	{
		public int FlippedUniform;
		
		public BlurShader(string v, string f) : base(v,f){}
		
		public override void Combine(){
			ShaderID = GL.CreateProgram();
			
			GL.BindAttribLocation(ShaderID, 0, "position");
			
			GL.AttachShader(ShaderID, ShaderVID);
			GL.AttachShader(ShaderID, ShaderFID);
			
			GL.LinkProgram(ShaderID);
			
			ShaderManager.RegisterShader(this);
			GetUniformsLocations();
		}
		public override void GetUniformsLocations(){
			this.FlippedUniform = GL.GetUniformLocation(ShaderID, "Flipped");
		}
	}
}
