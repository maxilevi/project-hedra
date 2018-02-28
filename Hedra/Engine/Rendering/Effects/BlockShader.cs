/*
 * Author: Zaphyk
 * Date: 25/02/2016
 * Time: 05:07 a.m.
 *
 */
using System;
using Hedra.Engine.Rendering;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Management;

namespace Hedra.Engine.Rendering.Effects
{
	/// <summary>
	/// Description of PPLShader.
	/// </summary>
	public class BlockShader : Shader
	{
		public int FogSettingsSize;
		public int FogSettingsIndex;
		public int PlayerPositionUniform;	
		public int CircleAreaUniform;
		public int AreaColorUniform;
		
		public BlockShader(string s1, string s2): base(s1,s2){}
		
		public override void Combine(){
			ShaderID = GL.CreateProgram();

			GL.AttachShader(ShaderID, ShaderVID);
			GL.AttachShader(ShaderID, ShaderFID);
			
			GL.LinkProgram(ShaderID);
			
			ShaderManager.RegisterShader(this);
			this.GetUniformsLocations();
		}
		public override void GetUniformsLocations(){
			PlayerPositionUniform = GL.GetUniformLocation(ShaderID, "PlayerPosition");
			FogSettingsIndex = GL.GetUniformBlockIndex(ShaderID, "FogSettings");
			GL.GetActiveUniformBlock(ShaderID, FogSettingsIndex, ActiveUniformBlockParameter.UniformBlockDataSize, out FogSettingsSize);
			CircleAreaUniform = GL.GetUniformLocation(ShaderID, "CircleArea");
			AreaColorUniform = GL.GetUniformLocation(ShaderID, "AreaColor");
		}	
	}
}
