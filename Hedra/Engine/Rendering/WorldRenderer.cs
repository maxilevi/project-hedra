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
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Generation.ChunkSystem;

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
	    public static Shader WaterShader { get; private set; }
	    public static Shader StaticShader { get; private set; }
        private static float WaveMovement { get; set; }
        public static WorldBuffer StaticBuffer;
	    public static WorldBuffer WaterBuffer;
		public static bool ShowWaterBackfaces {get; set;}
	    public static Texture3D NoiseTexture;

	    public static void AllocateMemory()
	    {
	        WaterShader = Shader.Build("Shaders/Water.vert", "Shaders/Water.frag");
	        StaticShader = Shader.Build("Shaders/Static.vert", "Shaders/Static.frag");
            StaticBuffer = new WorldBuffer(PoolSize.SuperBig);
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

	    public static void PrepareRendering()
	    {
	        //GameManager.Player.View.BuildCameraMatrix();
	        DrawManager.FrustumObject.SetFrustum(GameManager.Player.View.ModelViewMatrix);
        }

		public static void Render(Dictionary<Vector2, Chunk> ToDraw, ChunkBufferTypes Type){
            
			if(ToDraw.Count == 0) return;
			
			if(Type == ChunkBufferTypes.STATIC){
				IntPtr[] Offsets, ShadowOffsets;
				int[] Counts = StaticBuffer.BuildCounts(ToDraw, out Offsets);
				int[] ShadowCounts = StaticBuffer.BuildCounts(ToDraw, out ShadowOffsets, true);
				
				StaticBuffer.Data.Bind();
				GraphicsLayer.EnableVertexAttribArray(0);
			    GraphicsLayer.EnableVertexAttribArray(1);

			    GL.BindBuffer(StaticBuffer.Indices.Buffer.BufferTarget, StaticBuffer.Indices.Buffer.ID);

                if (GameSettings.Shadows){

					ShadowRenderer.Bind();
                    GraphicsLayer.MultiDrawElements(PrimitiveType.Triangles, ShadowCounts, DrawElementsType.UnsignedInt, ShadowOffsets, ShadowCounts.Length);
					ShadowRenderer.UnBind();

				}
				StaticBind();
				
				GraphicsLayer.EnableVertexAttribArray(2);

			    GraphicsLayer.MultiDrawElements(PrimitiveType.Triangles, Counts, DrawElementsType.UnsignedInt, Offsets, Counts.Length);
			    
			    GraphicsLayer.DisableVertexAttribArray(0);
				GraphicsLayer.DisableVertexAttribArray(1);
				GraphicsLayer.DisableVertexAttribArray(2);
				
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
				
				GraphicsLayer.EnableVertexAttribArray(0);
				GraphicsLayer.EnableVertexAttribArray(1);
				GraphicsLayer.EnableVertexAttribArray(2);
				
				GL.BindBuffer(WaterBuffer.Indices.Buffer.BufferTarget, WaterBuffer.Indices.Buffer.ID);
			    GraphicsLayer.MultiDrawElements(PrimitiveType.Triangles, Counts, DrawElementsType.UnsignedInt, Offsets, Counts.Length);
				
				GraphicsLayer.DisableVertexAttribArray(0);
				GraphicsLayer.DisableVertexAttribArray(1);
				GraphicsLayer.DisableVertexAttribArray(2);	
				
				WaterBuffer.Data.UnBind();
				WaterUnBind();
			}	
		}

		public static void Discard(){
			StaticBuffer.Discard();
			WaterBuffer.Discard();
		}

	    public static void ForceDiscard()
	    {
	        StaticBuffer.ForceDiscard();
	        WaterBuffer.ForceDiscard();
        }
		
		#region Binds
		
		private static void StaticBind(){
			GraphicsLayer.Disable(EnableCap.Blend);
			StaticShader.Bind();

            StaticShader["PlayerPosition"] = GameManager.Player.Position;
		    StaticShader["Time"] = !GameManager.InStartMenu ? Time.CurrentFrame : Time.UnPausedCurrentFrame;
		    StaticShader["Fancy"] = GameSettings.Fancy ? 1.0f : 0.0f;
			StaticShader["Snow"] = SkyManager.Snowing ? 1.0f : 0.0f;		
			StaticShader["UseShadows"] = (float) GameSettings.ShadowQuality * (GameSettings.Shadows ? 1f : 0f);
		    StaticShader["BakedOffset"] = BakedOffset;
		    StaticShader["Scale"] = Scale;
		    StaticShader["Offset"] = Offset;
		    StaticShader["Dither"] = Dither ? 1 : 0;
		    StaticShader["DitherRadius"] = DitherRadius;
            StaticShader["TransformationMatrix"] = TransformationMatrix;
            StaticShader["AreaPositions"] = World.Highlighter.AreaPositions;
			StaticShader["AreaColors"] = World.Highlighter.AreaColors;
			
            GL.ActiveTexture(TextureUnit.Texture1);
            GL.BindTexture(TextureTarget.Texture3D, NoiseTexture.Id);
		    StaticShader["noiseTexture"] = 1;
            
            if (GameSettings.Shadows){
				StaticShader["ShadowMVP"] = ShadowRenderer.ShadowMvp;
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, ShadowRenderer.ShadowFbo.TextureID[0]);
                StaticShader["ShadowTex"] = 0;
                StaticShader["ShadowDistance"] = ShadowRenderer.ShadowDistance;
			}		
		}
		
		private static void StaticUnBind(){
			StaticShader.UnBind();
		}
		
		private static void WaterBind(){
			GraphicsLayer.Enable(EnableCap.Blend);
			GL.BlendEquation(BlendEquationMode.FuncAdd);
           	GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
           	GraphicsLayer.Enable(EnableCap.Texture2D);
           	
           	WaterShader.Bind();
            WaterShader["PlayerPosition"] = GameManager.Player.Position;
           	
           	GL.ActiveTexture(TextureUnit.Texture0);
		    GL.BindTexture(TextureTarget.Texture2D, GameSettings.SSAO && GameSettings.Fancy ? DrawManager.MainBuffer.Ssao.FirstPass.TextureID[1] : 0);

		    WaterShader["Dither"] = Dither ? 1 : 0;
		    WaterShader["TransformationMatrix"] = TransformationMatrix;
            WaterShader["BakedOffset"] = BakedOffset;
		    WaterShader["Scale"] = Scale;
		    WaterShader["Offset"] = Offset + Vector3.UnitY;
            WaterShader["AreaPositions"] = World.Highlighter.AreaPositions;
			WaterShader["AreaColors"] = World.Highlighter.AreaColors;		
			WaterShader["WaveMovement"] = WaveMovement;
           	
           	if(ShowWaterBackfaces) GraphicsLayer.Disable(EnableCap.CullFace);
		}
		
		private static void WaterUnBind(){
			GraphicsLayer.Disable(EnableCap.Blend);
			GraphicsLayer.Disable(EnableCap.Texture2D);
			GraphicsLayer.Enable(EnableCap.CullFace);
		    WaterShader.UnBind();
        }

        #endregion

	    public static float DitherRadius { get; set; } = 1f;
        public static bool Dither { get; set; } = false;
        public static bool EnableCulling { get; set; } = true;
        public static Vector3 BakedOffset { get; set; }
        public static Vector3 Scale { get; set; } = Vector3.One;
        public static Vector3 Offset { get; set; }
	    public static Matrix4 TransformationMatrix { get; set; } = Matrix4.Identity;
	}
}
