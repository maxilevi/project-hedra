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
using OpenToolkit.Mathematics;
using System.Collections.Generic;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.Core;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.IO;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.Frustum;
using Hedra.Engine.Windowing;
using Hedra.Game;
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
        public static Shader WaterShader { get; private set; }
        public static Shader StaticShader { get; private set; }
        public static float WaveMovement { get; private set; }
        public static BufferBalancer StaticBuffer { get; private set; }
        public static BufferBalancer InstanceBuffer { get; private set; }
        public static BufferBalancer WaterBuffer { get; private set; }
        public static bool ShowWaterBackfaces {get; set;}
        public static Texture3D NoiseTexture { get; private set; }
        public static Texture DuDvMap { get; private set; }
        public static Texture NormalMap { get; private set; }
        private static IntPtr[] _shadowOffsets;
        private static uint[] _shadowCounts;

        public static void Initialize()
        {
            WaterShader = Shader.Build("Shaders/Water.vert", "Shaders/Water.frag");
            StaticShader = Shader.Build("Shaders/Static.vert", "Shaders/Static.frag");

            OpenSimplexNoise.Load(123);
            var noiseValues = CreateNoiseArray(out var width, out var height, out var depth);
            Log.WriteLine("Creating 3D noise texture...");
            NoiseTexture = new Texture3D(noiseValues, width, height, depth);
            Log.WriteLine("Loading DuDvMap...");
            DuDvMap = new Texture("Assets/FX/waterDuDvMap.png", true);
            Log.WriteLine("Loading water normal map...");
            NormalMap = new Texture("Assets/FX/waterNormalMap.png", true);
        }

        public static float[] CreateNoiseArray(out int Width, out int Height, out int Depth)
        {
            const int width = 16, height = 16, depth = 16;
            var noiseValues = new float[width * height * depth];
            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    for (var z = 0; z < depth; z++)
                    {
                        noiseValues[x * depth * height + y * depth + z] = (float)OpenSimplexNoise.Evaluate(x * 0.6f,y * 0.6f,z * 0.6f) * .5f + .5f;
                    }
                }
            }

            Width = width;
            Height = height;
            Depth = depth;
            return noiseValues;
        }

        public static void Allocate()
        {
            StaticBuffer = new BufferBalancer(
                new WorldBuffer(PoolSize.Normal),
                new WorldBuffer(PoolSize.Normal),
                new WorldBuffer(PoolSize.Normal)
            );
            InstanceBuffer = new BufferBalancer(
                new WorldBuffer(PoolSize.Normal),
                new WorldBuffer(PoolSize.Normal),
                new WorldBuffer(PoolSize.VerySmall)
            );
            WaterBuffer = new BufferBalancer(
                new WorldBuffer(PoolSize.Normal)
            );
            _shadowOffsets = new IntPtr[GeneralSettings.MaxChunks];
            _shadowCounts = new uint[GeneralSettings.MaxChunks];
        }

        public static void PrepareCameraMatrix()
        {
            Culling.BuildFrustum(GameManager.Player.View.ModelViewMatrix);
        }
        
        public static void PrepareShadowMatrix()
        {
            Culling.Frustum.SetMatrices(ShadowRenderer.DepthProj, ShadowRenderer.DepthView);
        }

        public static void Render(Dictionary<Vector2, Chunk> ToDraw, Dictionary<Vector2, Chunk> ToDrawShadow, WorldRenderType Type)
        {
            if(ToDraw.Count == 0 || GameSettings.HideWorld) return;
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
            StaticShader["Dither"] = GameSettings.SmoothLod ? 1 : 0;
            StaticShader["MaxDitherDistance"] = GameSettings.Quality ? GeneralSettings.MaxLod2DitherDistance : GeneralSettings.MaxLod1DitherDistance;
            StaticShader["MinDitherDistance"] = GameSettings.Quality ? GeneralSettings.MinLod2DitherDistance : GeneralSettings.MinLod1DitherDistance;
            
            InstanceBuffer.Draw(ToDraw);
        }
        
        private static void TerrainDraw(Dictionary<Vector2, Chunk> ToDraw, Dictionary<Vector2, Chunk> ShadowDraw)
        {
            if (GameSettings.Shadows)
            {
                ShadowRenderer.Bind();
                StaticBuffer.DrawShadows(ShadowDraw, ref _shadowOffsets, ref _shadowCounts);
                ShadowRenderer.UnBind();
            }
            StaticBind();
            StaticBuffer.Draw(ToDraw);
        }

        private static void WaterDraw(Dictionary<Vector2, Chunk> ToDraw)
        {
            WaveMovement += Time.IndependentDeltaTime * Mathf.Radian * 32;
            //if (WaveMovement >= 5f)
            //    WaveMovement = 0;

            WaterBind();

            WaterBuffer.Draw(ToDraw);
                
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
            InstanceBuffer.Discard();
        }

        public static void ForceDiscard()
        {
            StaticBuffer.ForceDiscard();
            WaterBuffer.ForceDiscard();
            InstanceBuffer.ForceDiscard();
        }
        
        #region Binds
        
        private static void StaticBind()
        {
            Renderer.Disable(EnableCap.Blend);
            StaticShader.Bind();

            StaticShader["PlayerPosition"] = GameManager.Player.Position;
            StaticShader["Dither"] = 0;
            StaticShader["BakedOffset"] = BakedOffset;
            StaticShader["TimeFancyShadowDistanceUseShadows"] = new Vector4(
                !GameManager.InStartMenu ? Time.AccumulatedFrameTime : Time.IndependentAccumulatedFrameTime,
                GameSettings.Quality ? 1.0f : 0.0f,
                ShadowRenderer.ShadowDistance,
                GameSettings.ShadowQuality * (GameSettings.Shadows ? 1f : 0f)
            );
            StaticShader["Scale"] = Scale;
            StaticShader["Offset"] = Offset;
            StaticShader["TransformationMatrix"] = TransformationMatrix;
            
            var highlights = World.Highlighter.Highlights.Where(H => !H.OnlyWater).ToArray();
            StaticShader["AreaCount"] = highlights.Length;
            StaticShader["AreaPositions"] = highlights.Select(H => H.AreaPosition).ToArray();
            StaticShader["AreaColors"] = highlights.Select(H => H.AreaColor).ToArray();
            
            Renderer.ActiveTexture(TextureUnit.Texture1);
            Renderer.BindTexture(TextureTarget.Texture3D, NoiseTexture.Id);
            StaticShader["noiseTexture"] = 1;
            
            if (GameSettings.Shadows)
            {
                StaticShader["ShadowMVP"] = ShadowRenderer.ShadowMvp;
                Renderer.ActiveTexture(TextureUnit.Texture0);
                Renderer.BindTexture(TextureTarget.Texture2D, ShadowRenderer.ShadowFbo.TextureId[0]);
                StaticShader["ShadowTex"] = 0;
            }        
        }
        
        private static void StaticUnBind()
        {
            /* Clear the texture units. */
            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
            Renderer.ActiveTexture(TextureUnit.Texture1);
            Renderer.BindTexture(TextureTarget.Texture3D, 0);
            StaticShader.Unbind();
        }
        
        private static void WaterBind()
        {
            Renderer.BlendEquation(BlendEquationMode.FuncAdd);
            Renderer.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            if(!GameSettings.UseSSR)
                Renderer.Enable(EnableCap.Blend);

            WaterShader.Bind();
            WaterShader["PlayerPosition"] = GameManager.Player.Position;

            WaterShader["depthMap"] = 0;
            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, GameSettings.SSAO && GameSettings.Quality ? DrawManager.MainBuffer.Ssao.FirstPass.TextureId[1] : 0);
               
            WaterShader["dudvMap"] = 1;
            Renderer.ActiveTexture(TextureUnit.Texture1);
            Renderer.BindTexture(TextureTarget.Texture2D, DuDvMap.Id);
            
            WaterShader["normalMap"] = 2;
            Renderer.ActiveTexture(TextureUnit.Texture2);
            Renderer.BindTexture(TextureTarget.Texture2D, NormalMap.Id);

            WaterShader["TransformationMatrix"] = TransformationMatrix;
            WaterShader["BakedOffset"] = BakedOffset;
            WaterShader["Scale"] = Scale;
            WaterShader["Offset"] = Offset;
            WaterShader["AreaCount"] = World.Highlighter.AreaCount;
            WaterShader["AreaPositions"] = World.Highlighter.AreaPositions;
            WaterShader["AreaColors"] = World.Highlighter.AreaColors;        
            WaterShader["WaveMovement"] = WaveMovement;
            WaterShader["Smoothness"] = WaterSmoothness;
            WaterShader["useSSR"] = GameSettings.UseSSR ? 1f : 0f;

            //if (ShowWaterBackfaces) Renderer.Disable(EnableCap.CullFace);
        }
        
        private static void WaterUnBind()
        {
            Renderer.ActiveTexture(TextureUnit.Texture0);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
            Renderer.ActiveTexture(TextureUnit.Texture1);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
            Renderer.ActiveTexture(TextureUnit.Texture2);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
            Renderer.ActiveTexture(TextureUnit.Texture3);
            Renderer.BindTexture(TextureTarget.Texture2D, 0);
            
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
