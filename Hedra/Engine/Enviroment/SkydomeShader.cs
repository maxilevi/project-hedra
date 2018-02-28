/*
 * Author: Zaphyk
 * Date: 19/02/2016
 * Time: 03:29 p.m.
 *
 */
using System;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Rendering;
using Hedra.Engine.Management;

namespace Hedra.Engine.Enviroment
{
	/// <summary>
	/// Description of SkyboxShader.
	/// </summary>
	public class SkydomeShader : Shader
	{
		public int SkyColorUniformLocation;
		public int TopColorUniformLocation;
		public int BotColorUniformLocation;
		public int HeightUniformLocation;
		public int TransMatrixUniformLocation;
		
		public SkydomeShader(string fileSourceV, string sourceF) : base(fileSourceV, sourceF){}
		public override void Combine(){
			ShaderID = GL.CreateProgram();
			
			GL.BindAttribLocation(ShaderID,0, "position");
			
			GL.AttachShader(ShaderID, ShaderVID);
			GL.AttachShader(ShaderID, ShaderFID);
			
			GL.LinkProgram(ShaderID);
			
			ShaderManager.RegisterShader(this);
			this.GetUniformsLocations();
		}
		
		public override void GetUniformsLocations(){
			TransMatrixUniformLocation = GL.GetUniformLocation(ShaderID, "TransMatrix");
			HeightUniformLocation = GL.GetUniformLocation(ShaderID, "Height");
			TopColorUniformLocation = GL.GetUniformLocation(ShaderID, "TopColor");
			BotColorUniformLocation = GL.GetUniformLocation(ShaderID, "BotColor");
		}
	}
}