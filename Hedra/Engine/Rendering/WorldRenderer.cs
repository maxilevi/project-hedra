/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/09/2017
 * Time: 03:13 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Collections.Generic;
using Hedra.Engine.Enviroment;
using Hedra.Engine.Rendering.Effects;

namespace Hedra.Engine.Rendering
{
	/// <summary>
	/// Description of WorldRenderer.
	/// </summary>
	public static class WorldRenderer
	{
	    public const int NoShadowsFlag = -1;
	    public const int NoHighlightFlag = -2;
	    public const int NoShadowsAndNoHighlightFlag = -3;
	    public static WorldBuffer StaticBuffer;
	    public static WorldBuffer WaterBuffer;
	    private static float WaveMovement { get; set; }
        public static float WaterTransparencyModifier {get; set;}
		public static bool ShowWaterBackfaces {get; set;}
	    public static Texture3D NoiseTexture;

	    public static void AllocateMemory()
	    {
	        StaticBuffer = new WorldBuffer(PoolSize.Normal);
            WaterBuffer = new WorldBuffer(PoolSize.Tiny);

	        var noiseValues = new float[16, 16, 16];
	        for (var x = 0; x < noiseValues.GetLength(0); x++)
	        {
	            for (var y = 0; y < noiseValues.GetLength(1); y++)
	            {
	                for (var z = 0; z < noiseValues.GetLength(2); z++)
	                {
	                    noiseValues[x,y,z] = (float) OpenSimplexNoise.Evaluate(x * 0.6f,y * 0.6f,z * 0.6f) * .5f + .5f;
	                }
                }
            }
	        NoiseTexture = new Texture3D(noiseValues);

        }

		public static void Render(Dictionary<Vector2, Chunk> ToDraw, ChunkBufferTypes Type){
            
			if(ToDraw.Count == 0) return;
			
			Scenes.SceneManager.Game.LPlayer.View.RebuildMatrix();
			DrawManager.FrustumObject.SetFrustum(Scenes.SceneManager.Game.LPlayer.View.Matrix);
			
			if(Type == ChunkBufferTypes.STATIC){
				IntPtr[] Offsets, ShadowOffsets;
				int[] Counts = StaticBuffer.BuildCounts(ToDraw, out Offsets);
				int[] ShadowCounts = StaticBuffer.BuildCounts(ToDraw, out ShadowOffsets, true);
				
				StaticBuffer.Data.Bind();
				GL.EnableVertexAttribArray(0);
			    GL.EnableVertexAttribArray(1);

			    GL.BindBuffer(StaticBuffer.Indices.Buffer.BufferTarget, StaticBuffer.Indices.Buffer.ID);

                if (GameSettings.Shadows){

					ShadowRenderer.Bind();
                    GraphicsLayer.MultiDrawElements(PrimitiveType.Triangles, ShadowCounts, DrawElementsType.UnsignedInt, ShadowOffsets, ShadowCounts.Length);
					ShadowRenderer.UnBind();

				}
				StaticBind();
				
				GL.EnableVertexAttribArray(2);

			    GraphicsLayer.MultiDrawElements(PrimitiveType.Triangles, Counts, DrawElementsType.UnsignedInt, Offsets, Counts.Length);
			    
			    GL.DisableVertexAttribArray(0);
				GL.DisableVertexAttribArray(1);
				GL.DisableVertexAttribArray(2);
				
				StaticBuffer.Data.UnBind();
				
				StaticUnBind();
			}
			else if(Type == ChunkBufferTypes.WATER){
			
				IntPtr[] Offsets;
				int[] Counts = WaterBuffer.BuildCounts(ToDraw, out Offsets);

			    WaveMovement += (float)Time.unScaledDeltaTime * Mathf.Radian * 64;
			    if (WaveMovement >= 5)
			        WaveMovement = 0;

                WaterBind();
				WaterBuffer.Data.Bind();
				
				GL.EnableVertexAttribArray(0);
				GL.EnableVertexAttribArray(1);
				GL.EnableVertexAttribArray(2);
				
				GL.BindBuffer(WaterBuffer.Indices.Buffer.BufferTarget, WaterBuffer.Indices.Buffer.ID);
			    GraphicsLayer.MultiDrawElements(PrimitiveType.Triangles, Counts, DrawElementsType.UnsignedInt, Offsets, Counts.Length);
				
				GL.DisableVertexAttribArray(0);
				GL.DisableVertexAttribArray(1);
				GL.DisableVertexAttribArray(2);	
				
				WaterBuffer.Data.UnBind();
				WaterUnBind();
			}	
		}

		public static void Clear(){
			StaticBuffer.Clear();
			WaterBuffer.Clear();
		}
		
		#region Binds
		
		private static void StaticBind(){
			GL.Disable(EnableCap.Blend);
			BlockShaders.StaticShader.Bind();
			GL.Uniform3(BlockShaders.StaticShader.LightColorLocation, ShaderManager.LightColor);
			GL.Uniform3(BlockShaders.StaticShader.PlayerPositionUniform, Scenes.SceneManager.Game.LPlayer.Position);
			if(Constants.CHARACTER_CHOOSED)
				GL.Uniform1(BlockShaders.StaticShader.TimeUniform, Time.CurrentFrame );
			else
				GL.Uniform1(BlockShaders.StaticShader.TimeUniform, Time.UnPausedCurrentFrame );
			GL.Uniform1(BlockShaders.StaticShader.FancyUniform, (GameSettings.Fancy) ? 1.0f : 0.0f);
			GL.Uniform1(BlockShaders.StaticShader.SnowUniform, (SkyManager.Snowing) ? 1.0f : 0.0f);
			
			GL.Uniform1(BlockShaders.StaticShader.UseShadowsUniform, (float) GameSettings.ShadowQuality);

			BlockShaders.StaticShader.AreaPositionsUniform.LoadVectorArray(World.Highlighter.AreaPositions);
			BlockShaders.StaticShader.AreaColorsUniform.LoadVectorArray(World.Highlighter.AreaColors);
			
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture3D, NoiseTexture.Id);
		    GL.Uniform1(BlockShaders.StaticShader.NoiseTexUniform, 1);
            
            if (GameSettings.Shadows){
				GL.UniformMatrix4(BlockShaders.StaticShader.ShadowMVPUniform, false, ref ShadowRenderer.ShadowMVP);
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, ShadowRenderer.ShadowFBO.TextureID[0]);
				GL.Uniform1(BlockShaders.StaticShader.ShadowTexUniform, 0);
			    GL.Uniform1(BlockShaders.StaticShader.ShadowDistanceUniform, ShadowRenderer.ShadowDistance);
			}
			
		}
		
		private static void StaticUnBind(){
			BlockShaders.StaticShader.UnBind();
		}
		
		private static void WaterBind(){
			GL.Enable(EnableCap.Blend);
			GL.BlendEquation(BlendEquationMode.FuncAdd);
           	GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
           	GL.Enable(EnableCap.Texture2D);
           	
           	BlockShaders.WaterShader.Bind();
           	GL.Uniform3(BlockShaders.WaterShader.PlayerPositionUniform, Scenes.SceneManager.Game.LPlayer.Position);
           	GL.Uniform3(BlockShaders.WaterShader.LightColorLocation, ShaderManager.LightColor);
           	
           	GL.ActiveTexture(TextureUnit.Texture0);
		    GL.BindTexture(TextureTarget.Texture2D, (GameSettings.SSAO) ? DrawManager.MainBuffer.SSAO.FirstPass.TextureID[1] : DrawManager.MainBuffer.Default.TextureID[0]);
			
			BlockShaders.WaterShader.AreaPositionsUniform.LoadVectorArray(World.Highlighter.AreaPositions);
			BlockShaders.WaterShader.AreaColorsUniform.LoadVectorArray(World.Highlighter.AreaColors);
			
			GL.Uniform1(BlockShaders.WaterShader.WaveMovementUniform, WaveMovement);
			GL.Uniform1(BlockShaders.WaterShader.TransparencyLocation, WaterTransparencyModifier);
           	
           	
           	if(ShowWaterBackfaces) 
           		GL.Disable(EnableCap.CullFace);
		}
		
		private static void WaterUnBind(){
			GL.Disable(EnableCap.Blend);
			GL.Disable(EnableCap.Texture2D);
			BlockShaders.WaterShader.UnBind();
			GL.Enable(EnableCap.CullFace);
		}
		
		#endregion
		
	}
}
