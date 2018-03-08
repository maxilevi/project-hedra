/*
 * Author: Zaphyk
 * Date: 21/02/2016
 * Time: 05:21 p.m.
 *
 */
using System;
using System.IO;
using Hedra.Engine.Management;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Rendering.Effects
{
	/// <summary>
	/// Description of WaterShader.
	/// </summary>
	public class WaterShader : Shader
	{
		public int WaveHeightUniform;
		public int WaterLevelUniform;
		public int WaveWidthUniform;
		public int WaveTimeUniform;
		public int WaveMovementUniform;
		public int ReflectionTextureUniform;
		public int DuDvMapUniform;
		public uint DuDvMap;
		public int BlockSizeLocation;
		public int PlayerPositionUniform;
		public UniformVector4Array AreaPositionsUniform, AreaColorsUniform;
		
		public WaterShader(string s, string s2) : base(s, s2){}
		
		public override void GetUniformsLocations(){
			PlayerPositionUniform = GL.GetUniformLocation(ShaderID, "PlayerPosition");
			WaveHeightUniform = GL.GetUniformLocation(ShaderID, "WaveHeight");
			WaterLevelUniform = GL.GetUniformLocation(ShaderID, "WaterLevel");
			WaveWidthUniform = GL.GetUniformLocation(ShaderID, "WaveWidth");
			WaveTimeUniform = GL.GetUniformLocation(ShaderID, "WaveTime");
			WaveMovementUniform = GL.GetUniformLocation(ShaderID, "WaveMovement");
			ReflectionTextureUniform = GL.GetUniformLocation(ShaderID, "ReflectionSampler");
			DuDvMapUniform = GL.GetUniformLocation(ShaderID, "DuDvMap");
			BlockSizeLocation = GL.GetUniformLocation(ShaderID, "BlockSize");
			AreaPositionsUniform = new UniformVector4Array(ShaderID, "AreaPositions", 16);
			AreaColorsUniform = new UniformVector4Array(ShaderID, "AreaColors", 16);
			
		}
	}
}
