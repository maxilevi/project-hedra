/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/11/2016
 * Time: 02:10 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
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
	public class StaticShader : Shader
	{
		public int FogSettingsSize;
		public int FogSettingsIndex;
		public int PlayerPositionUniform, TimeUniform, FancyUniform, SnowUniform, ShadowMVPUniform, ShadowTexUniform, UseShadowsUniform;	
		public UniformVector4Array AreaPositionsUniform, AreaColorsUniform;
		public int BaseTextureUniform, ShadowDistanceUniform, NoiseTexUniform;
		
		public StaticShader(string s1, string s2): base(s1,s2){}
		
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
			TimeUniform = GL.GetUniformLocation(ShaderID, "Time");
			FancyUniform = GL.GetUniformLocation(ShaderID, "Fancy");
			SnowUniform = GL.GetUniformLocation(ShaderID, "Snow");
			ShadowTexUniform = GL.GetUniformLocation(ShaderID, "ShadowTex");
		    NoiseTexUniform = GL.GetUniformLocation(ShaderID, "noiseTexture");
            ShadowMVPUniform = GL.GetUniformLocation(ShaderID, "ShadowMVP");
			UseShadowsUniform = GL.GetUniformLocation(ShaderID, "UseShadows");
			AreaPositionsUniform = new UniformVector4Array(ShaderID, "AreaPositions", 16);
			AreaColorsUniform = new UniformVector4Array(ShaderID, "AreaColors", 16);
			BaseTextureUniform = GL.GetUniformLocation(ShaderID, "BaseTexture");
            ShadowDistanceUniform = GL.GetUniformLocation(ShaderID, "ShadowDistance");

		}
	}
}
