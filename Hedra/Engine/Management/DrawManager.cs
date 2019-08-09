/*
 * Author: Zaphyk
 * Date: 04/02/2016
 * Time: 05:35 p.m.
 *
 */
using System.Collections.Generic;
using System.Linq;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.IO;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Core;
using Hedra.Engine.Rendering.Effects;
using Hedra.Engine.Rendering.Frustum;
using Hedra.Engine.Rendering.UI;
using OpenTK.Graphics.OpenGL4;

namespace Hedra.Engine.Management
{
    /// <summary>
    /// A static class which acts as a layer between every Draw() method and the actual rendeirng 
    /// </summary>
    public static class DrawManager
    {
        private static readonly object Lock = new object();
        private static readonly object TransparentLock = new object();
        private static readonly HashSet<IRenderable> DrawFunctionsSet;
        private static readonly List<IRenderable> DrawFunctions;
        private static readonly List<IRenderable> TransparentObjects;
        private static bool _initialized;
        private static CursorIcon _mouseCursorIcon;
        
        public static List<IRenderable> ParticleRenderer { get; }
        public static List<IRenderable> TrailRenderer { get; }
        public static GUIRenderer UIRenderer { get; }
        public static DropShadowRenderer DropShadows { get; }
        public static int CullableObjectsCount { get; private set; }
        public static int CulledObjectsCount { get; private set; }
        public static int DrawCalls { get; private set; }
        public static int VertsCount { get; private set; }
        public static MainFBO MainBuffer { get; private set; }

        static DrawManager()
        {
            DrawFunctionsSet = new HashSet<IRenderable>();
            DrawFunctions = new List<IRenderable>();
            ParticleRenderer = new List<IRenderable>();
            TrailRenderer = new List<IRenderable>();
            TransparentObjects = new List<IRenderable>();
            UIRenderer = new GUIRenderer();
            DropShadows = new DropShadowRenderer();
        }

        public static void Add(IRenderable Renderable)
        {
            lock (Lock)
            {
                DrawFunctionsSet.Add(Renderable);
                DrawFunctions.Add(Renderable);
                CullableObjectsCount = DrawFunctions.Sum(D => D is ICullable ? 1 : 0);
            }
        }
        
        public static void Remove(IRenderable Renderable)
        {
            lock (Lock)
            {
                DrawFunctionsSet.Remove(Renderable);
                DrawFunctions.Remove(Renderable);
            }
        }

        public static void AddTransparent(IRenderable Renderable)
        {
            lock (TransparentLock)
                TransparentObjects.Add(Renderable);
        }

        public static void RemoveTransparent(IRenderable Renderable)
        {
            lock (TransparentLock)
                TransparentObjects.Remove(Renderable);
        }

        private static void BulkDraw()
        {
            SkyManager.Draw();
            World.CullTest();
            World.Draw(WorldRenderType.StaticAndInstance);
            World.OccludeTest();
            DropShadows.Draw();

            var drawedObjects = 0;
            var drawedCullableObjects = 0;
            lock (Lock)
            {
                for (var i = DrawFunctions.Count-1; i > -1; i--)
                {
                    if (DrawFunctions[i] == null) continue;

                    if (DrawFunctions[i] is ICullable cullable)
                    {
                        if (!Culling.IsInside(cullable)) continue;
                        DrawFunctions[i].Draw();
                        drawedObjects++;
                        drawedCullableObjects++;
                    }
                    else
                    {
                        DrawFunctions[i].Draw();
                        drawedObjects++;
                    }
                }
            }
        
            Renderer.Enable(EnableCap.DepthTest);
            World.Draw(WorldRenderType.Water);
            for (var i = TrailRenderer.Count - 1; i > -1; i--)
            {
                TrailRenderer[i].Draw();
            }
            for (var i = ParticleRenderer.Count-1; i > -1; i--)
            {
                ParticleRenderer[i].Draw();
            }
            BulletPhysics.BulletPhysics.Draw();
            lock (TransparentLock)
            {
                for (var i = TransparentObjects.Count - 1; i > -1; i--)
                {
                    if (TransparentObjects[i] is ICullable cullable)
                    {
                        if (!Culling.IsInside(cullable)) continue;
                        TransparentObjects[i].Draw();
                        drawedObjects++;
                        drawedCullableObjects++;
                    }
                    else
                    {
                        TransparentObjects[i].Draw();
                        drawedObjects++;
                    }
                }
            }

            var worldCalls = 3;//Water + Static + Shadows
            int dropShadowCalls = DropShadows.Count;//Ideally
            CulledObjectsCount = CullableObjectsCount - drawedCullableObjects;
            DrawCalls = drawedObjects + UIRenderer.DrawCount + worldCalls + dropShadowCalls;
        }
        
        public static void Draw()
        {    
             VertsCount = 0;
             if(MainBuffer.Enabled)
             {        
                MainBuffer.CaptureData();
                BulkDraw();
                MainBuffer.UnCaptureData();
             }
             MainBuffer.Draw();
            
            UIRenderer.Draw();
            
            MainBuffer.Clear();
            
            #if DEBUG
            {
                var code = Renderer.GetError();
                if(code != ErrorCode.NoError)
                    Log.WriteResult(false, "OpenGL error: "+code.ToString());
            }
            #endif
        }
         
         public static void Load()
         {
             MainBuffer = new MainFBO
             {
                 Enabled = true
             };
             _initialized = true;
         }

        public static void Dispose()
        {
            _mouseCursorIcon?.Dispose();
            MainBuffer.Dispose();
            UIRenderer.Dispose();
        }
    }
}