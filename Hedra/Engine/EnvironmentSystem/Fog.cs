/*
 * Author: Zaphyk
 * Date: 13/03/2016
 * Time: 08:39 p.m.
 *
 */
using System;
using OpenTK;
using System.Runtime.InteropServices;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Effects;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.EnvironmentSystem
{
	/// <summary>
	/// Description of Fog.
	/// </summary>
	public sealed class Fog
	{
		public uint UboId { get; }
		public FogData FogValues;
        public float MaxDistance { get; private set; }
	    public float MinDistance { get; private set; }
	    private readonly int _fogSettingsIndex;
        private readonly int _fogSettingsSize;

        public Fog()
        {
            _fogSettingsIndex = Renderer.GetUniformBlockIndex(WorldRenderer.StaticShader.ShaderId, "FogSettings");
            Renderer.GetActiveUniformBlock(
                WorldRenderer.StaticShader.ShaderId,
                _fogSettingsIndex, ActiveUniformBlockParameter.UniformBlockDataSize,
                out _fogSettingsSize
                );

            uint id;
			Renderer.GenBuffers(1, out id);
            UboId = id;
			
			Renderer.BindBuffer(BufferTarget.UniformBuffer, UboId);
            Renderer.BufferData(BufferTarget.UniformBuffer, (IntPtr)_fogSettingsSize, IntPtr.Zero, BufferUsageHint.DynamicDraw);

            this.BindBlock(_fogSettingsIndex);
		}
		
		public void BindBlock(int Index){
			Renderer.BindBuffer(BufferTarget.UniformBuffer, UboId);
			Renderer.BindBufferBase(BufferRangeTarget.UniformBuffer, Index, (int) UboId);
		}
		
		public void UpdateFogSettings(float MinDist, float MaxDist)
		{
		    MinDistance = MinDist;
		    MaxDistance = MaxDist;
			var data = new FogData(MinDist, MaxDist, GameSettings.Height * (1 - GameManager.Player.View.Pitch * .25f), SkyManager.Sky.BotColor, SkyManager.Sky.TopColor);
			FogValues = data;
			Renderer.BindBuffer(BufferTarget.UniformBuffer, UboId);
			Renderer.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, (IntPtr)_fogSettingsSize, ref data);
			
		}
		
		public void Enable(){
		    WorldRenderer.StaticShader.Bind();
		}
		public void Disable(){
		    WorldRenderer.StaticShader.Unbind();
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
