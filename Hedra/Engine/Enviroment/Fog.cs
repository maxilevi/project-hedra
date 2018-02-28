/*
 * Author: Zaphyk
 * Date: 13/03/2016
 * Time: 08:39 p.m.
 *
 */
using System;
using OpenTK;
using System.Runtime.InteropServices;
using Hedra.Engine.Rendering.Effects;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Enviroment
{
	/// <summary>
	/// Description of Fog.
	/// </summary>
	public sealed class Fog
	{
		public uint UboID;
		public FogData FogValues;
        public float MaxDistance { get; private set; }
	    public float MinDistance { get; private set; }

        public Fog(){
			GL.GenBuffers(1, out UboID);
			
			GL.BindBuffer(BufferTarget.UniformBuffer, UboID);
			GL.BufferData(BufferTarget.UniformBuffer, (IntPtr) (BlockShaders.StaticShader.FogSettingsSize), IntPtr.Zero, BufferUsageHint.DynamicDraw);
			
			BindBlock(BlockShaders.StaticShader.FogSettingsIndex);
		}
		
		public void BindBlock(int Index){
			GL.BindBuffer(BufferTarget.UniformBuffer, UboID);
			GL.BindBufferBase(BufferRangeTarget.UniformBuffer, Index, (int) UboID);
		}
		
		public void UpdateFogSettings(float MinDist, float MaxDist)
		{
		    MinDistance = MinDist;
		    MaxDistance = MaxDist;
			FogData Data = new FogData(MinDist, MaxDist, Constants.HEIGHT, SkyManager.Skydome.BotColor, SkyManager.Skydome.TopColor);
			FogValues = Data;
			GL.BindBuffer(BufferTarget.UniformBuffer, UboID);
			GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, (IntPtr) (BlockShaders.StaticShader.FogSettingsSize), ref Data);
			
		}
		
		public void Enable(){
			BlockShaders.StaticShader.Bind();
		}
		public void Disable(){
			BlockShaders.StaticShader.UnBind();
		}
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public struct FogData {
	    public Vector4 U_BotColor;
	    public Vector4 U_TopColor;
	   	public float MinDist;
	    public float MaxDist;
	    public float U_Height;
	    
	    public FogData(float MinDist, float MaxDist, float Height, Vector4 U_BotColor, Vector4 U_TopColor){
	    	this.MinDist = MinDist;
	    	this.MaxDist = MaxDist;
	    	this.U_Height = Height;
	    	this.U_BotColor = U_BotColor;
	    	this.U_TopColor = U_TopColor;
	    }
	};
}
