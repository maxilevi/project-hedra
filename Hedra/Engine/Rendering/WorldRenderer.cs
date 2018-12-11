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
using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using Hedra.Core;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Game;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Rendering;

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
        public static float WaveMovement { get; private set; }
        public static WorldBuffer StaticBuffer { get; private set; }
        public static WorldBuffer InstanceBuffer { get; private set; }
        public static WorldBuffer WaterBuffer { get; private set; }
        public static bool ShowWaterBackfaces {get; set;}
        public static Texture3D NoiseTexture { get; private set; }

        public static void AllocateMemory()
        {
            WaterShader = Shader.Build("Shaders/Water.vert", "Shaders/Water.frag");
            StaticShader = Shader.Build("Shaders/Static.vert", "Shaders/Static.frag");
            StaticBuffer = new WorldBuffer(PoolSize.SuperBig);
            InstanceBuffer = new WorldBuffer(PoolSize.Normal);
            WaterBuffer = new WorldBuffer(PoolSize.Tiny);

            var noiseValues = new float[32, 32, 32];
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
            DrawManager.FrustumObject.SetFrustum(GameManager.Player.View.ModelViewMatrix);
        }

        public static void Render(Dictionary<Vector2, Chunk> ToDraw, Dictionary<Vector2, Chunk> ToDrawShadow, WorldRenderType Type)
        {
            
            if(ToDraw.Count == 0) return;
            
            if((Type & WorldRenderType.Static) == WorldRenderType.Static)
            {
                TerrainDraw(ToDraw, ToDrawShadow);
                if ((Type & WorldRenderType.Instance) == WorldRenderType.Instance) InstanceDraw(ToDraw);
                StaticUnBind();
            }
            else if(Type == WorldRenderType.Water)
            {
                WaterDraw(ToDraw);
            }    
        }

        private static void InstanceDraw(Dictionary<Vector2, Chunk> ToDraw)
        {
            var counts = InstanceBuffer.BuildCounts(ToDraw, out var offsets);

            InstanceBuffer.Bind();
            InstanceBuffer.BindIndices();
            StaticShader["Dither"] = GameSettings.SmoothLod ? 1 : 0;
            StaticShader["MaxDitherDistance"] = GeneralSettings.MaxLodDitherDistance;
            StaticShader["MinDitherDistance"] = GeneralSettings.MinLodDitherDistance;
            
            Renderer.MultiDrawElements(PrimitiveType.Triangles, counts, DrawElementsType.UnsignedInt, offsets, counts.Length);
        }
        
        private static void TerrainDraw(Dictionary<Vector2, Chunk> ToDraw, Dictionary<Vector2, Chunk> ShadowDraw)
        {
            int[] Counts = StaticBuffer.BuildCounts(ToDraw, out IntPtr[]  Offsets);
            int[] ShadowCounts = StaticBuffer.BuildCounts(ShadowDraw, out IntPtr[] ShadowOffsets);
                
            StaticBuffer.Bind(false);
            Renderer.EnableVertexAttribArray(0);
            Renderer.EnableVertexAttribArray(1);

            StaticBuffer.BindIndices();

            if (GameSettings.Shadows)
            {
                ShadowRenderer.Bind();
                Renderer.MultiDrawElements(PrimitiveType.Triangles, ShadowCounts, DrawElementsType.UnsignedInt, ShadowOffsets, ShadowCounts.Length);
                ShadowRenderer.UnBind();
            }

            StaticBind();
            Renderer.EnableVertexAttribArray(2);
            Renderer.MultiDrawElements(PrimitiveType.Triangles, Counts, DrawElementsType.UnsignedInt, Offsets, Counts.Length);            

            StaticBuffer.Unbind();
        }

        private static void WaterDraw(Dictionary<Vector2, Chunk> ToDraw)
        {
            var counts = WaterBuffer.BuildCounts(ToDraw, out var Offsets);

            WaveMovement += Time.IndependantDeltaTime * Mathf.Radian * 32;
            if (WaveMovement >= 5f)
                WaveMovement = 0;

            WaterBind();
            WaterBuffer.Bind();
                
            Renderer.EnableVertexAttribArray(0);
            Renderer.EnableVertexAttribArray(1);
            Renderer.EnableVertexAttribArray(2);
                
            Renderer.BindBuffer(WaterBuffer.Indices.Buffer.BufferTarget, WaterBuffer.Indices.Buffer.ID);
            Renderer.MultiDrawElements(PrimitiveType.Triangles, counts, DrawElementsType.UnsignedInt, Offsets, counts.Length);
                
            Renderer.DisableVertexAttribArray(0);
            Renderer.DisableVertexAttribArray(1);
            Renderer.DisableVertexAttribArray(2);    
                
            WaterBuffer.Unbind();
            WaterUnBind();
        }

        public static void Remove(Vector2 ChunkOffset)
        {
            StaticBuffer.Remove(ChunkOffset);
            InstanceBuffer.Remove(ChunkOffset);
            WaterBuffer.Remove(ChunkOffset);
        }
        
        public static bool UpdateStatic(Vector2 Offset, VertexData Data)
        {
            return StaticBuffer.Update(Offset, Data);
        }

        public static bool UpdateWater(Vector2 Offset, VertexData Data)
        {
            return WaterBuffer.Update(Offset, Data);
        }

        public static bool UpdateInstance(Vector2 Offset, VertexData Data)
        {
            return InstanceBuffer.Update(Offset, Data);
        }

        public static void Discard()
        {
            StaticBuffer.Discard();
            WaterBuffer.Discard();
        }

        public static void ForceDiscard()
        {
            StaticBuffer.ForceDiscard();
            WaterBuffer.ForceDiscard();
        }
        
        #region Binds
        
        private static void StaticBind()
        {
            Renderer.Disable(EnableCap.Blend);
            StaticShader.Bind();

            StaticShader["PlayerPosition"] = GameManager.Player.Position;
            StaticShader["Time"] = !GameManager.InStartMenu ? Time.AccumulatedFrameTime : Time.IndependentAccumulatedFrameTime;
            //StaticShader["Fancy"] = GameSettings.Fancy ? 1.0f : 0.0f;
            //StaticShader["Snow"] = SkyManager.Snowing ? 1.0f : 0.0f;
            StaticShader["Dither"] = 0;
            StaticShader["UseShadows"] = (float) GameSettings.ShadowQuality * (GameSettings.Shadows ? 1f : 0f);
            StaticShader["BakedOffset"] = BakedOffset;
            StaticShader["Scale"] = Scale;
            StaticShader["Offset"] = Offset;
            StaticShader["TransformationMatrix"] = TransformationMatrix;
            StaticShader["AreaPositions"] = World.Highlighter.AreaPositions;
            StaticShader["AreaColors"] = World.Highlighter.AreaColors;
            
            Renderer.ActiveTexture(TextureUnit.Texture1);
            Renderer.BindTexture(TextureTarget.Texture3D, NoiseTexture.Id);
            StaticShader["noiseTexture"] = 1;
            
            if (GameSettings.Shadows)
            {
                StaticShader["ShadowMVP"] = ShadowRenderer.ShadowMvp;
                Renderer.ActiveTexture(TextureUnit.Texture0);
                Renderer.BindTexture(TextureTarget.Texture2D, ShadowRenderer.ShadowFbo.TextureID[0]);
                StaticShader["ShadowTex"] = 0;
                StaticShader["ShadowDistance"] = ShadowRenderer.ShadowDistance;
            }        
        }
        
        private static void StaticUnBind()
        {
            StaticShader.Unbind();
        }
        
        private static void WaterBind()
        {
            Renderer.BlendEquation(BlendEquationMode.FuncAdd);
            Renderer.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            Renderer.Enable(EnableCap.Blend);

            WaterShader.Bind();
            WaterShader["PlayerPosition"] = GameManager.Player.Position;
               
               Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, GameSettings.SSAO && GameSettings.Quality ? DrawManager.MainBuffer.Ssao.FirstPass.TextureID[1] : 0);

            WaterShader["TransformationMatrix"] = TransformationMatrix;
            WaterShader["BakedOffset"] = BakedOffset;
            WaterShader["Scale"] = Scale;
            WaterShader["Offset"] = Offset;
            WaterShader["AreaPositions"] = World.Highlighter.AreaPositions;
            WaterShader["AreaColors"] = World.Highlighter.AreaColors;        
            WaterShader["WaveMovement"] = WaveMovement;
            WaterShader["Smoothness"] = WaterSmoothness;

            if (ShowWaterBackfaces) Renderer.Disable(EnableCap.CullFace);
        }
        
        private static void WaterUnBind()
        {
            Renderer.Disable(EnableCap.Blend);
            Renderer.Enable(EnableCap.CullFace);
            WaterShader.Unbind();
        }

        #endregion
        public static float WaterSmoothness { get; set; } = 1f;
        public static bool EnableCulling { get; set; } = true;
        public static Vector3 BakedOffset { get; set; }
        public static Vector3 Scale { get; set; } = Vector3.One;
        public static Vector3 Offset { get; set; }
        public static Matrix4 TransformationMatrix { get; set; } = Matrix4.Identity;
    }

    [Flags]
    public enum WorldRenderType : byte
    {
        Static = 1,
        Water = 2,
        Instance = 4,
        StaticAndInstance = Static | Instance
    }
}
