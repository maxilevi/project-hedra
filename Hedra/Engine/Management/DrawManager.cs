/*
 * Author: Zaphyk
 * Date: 04/02/2016
 * Time: 05:35 p.m.
 *
 */

using System;
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
using Hedra.Engine.Rendering.Particles;
using Hedra.Engine.Rendering.UI;
using Hedra.Game;
using Hedra.Rendering;
using System.Numerics;
using Hedra.Engine.Core;
using Hedra.Engine.Windowing;

namespace Hedra.Engine.Management
{
    /// <summary>
    /// A static class which acts as a layer between every Draw() method and the actual rendeirng 
    /// </summary>
    public static class DrawManager
    {
        private static readonly object Lock = new object();
        private static readonly object TransparentLock = new object();
        private static readonly object ObjectMeshLock = new object();
        private static readonly HashSet<IRenderable> DrawFunctionsSet;
        private static readonly List<IRenderable> DrawFunctions;
        private static readonly List<IRenderable> TransparentObjects;
        private static readonly List<ObjectMesh> ObjectMeshes;
        private static bool _initialized;

        public static List<IRenderable> ParticleRenderer { get; }
        public static List<IRenderable> TrailRenderer { get; }
        public static GUIRenderer UIRenderer { get; }
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
            ObjectMeshes = new List<ObjectMesh>();
            UIRenderer = new GUIRenderer();
        }

        public static void Add(IRenderable Renderable)
        {
#if DEBUG
            if(Renderable is ObjectMesh || Renderable is TrailRenderer || Renderable is ParticleSystem) 
                throw new ArgumentOutOfRangeException($"This type of renderable ('{nameof(Renderable)}') is not supported");
#endif
            lock (Lock)
            {
                DrawFunctionsSet.Add(Renderable);
                DrawFunctions.Add(Renderable);
                CullableObjectsCount = DrawFunctions.Sum(D => D is ICullable ? 1 : 0);
            }
        }
        
        public static void Remove(IRenderable Renderable)
        {
#if DEBUG
            if(Renderable is ObjectMesh || Renderable is TrailRenderer || Renderable is ParticleSystem) 
                throw new ArgumentOutOfRangeException($"This type of renderable ('{nameof(Renderable)}') is not supported");
#endif
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

        public static void AddObjectMesh(ObjectMesh Mesh)
        {
            lock(ObjectMeshLock)
                ObjectMeshes.Add(Mesh);
        }

        public static void RemoveObjectMesh(ObjectMesh Mesh)
        {
            lock(ObjectMeshLock)
                ObjectMeshes.Remove(Mesh);
        }

        private static void SetupDrawing()
        {
            World.CullTest();
        }

        private static void BulkDraw()
        {
            SkyManager.Draw();
            World.Draw(WorldRenderType.StaticAndInstance);
            World.OccludeTest();

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

            lock (ObjectMeshLock)
                ObjectMeshBuffer.DrawBatched(ObjectMeshes);
            Renderer.Enable(EnableCap.DepthTest);
            if (!GameSettings.UseSSR) World.Draw(WorldRenderType.Water);
            for (var i = TrailRenderer.Count - 1; i > -1; i--)
            {
                TrailRenderer[i].Draw();
            }
            for (var i = ParticleRenderer.Count-1; i > -1; i--)
            {
                ParticleRenderer[i].Draw();
            }

            Culling.Draw();
            Bullet.BulletPhysics.Draw();
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
            CulledObjectsCount = CullableObjectsCount - drawedCullableObjects;
            DrawCalls = drawedObjects + UIRenderer.DrawCount + worldCalls;
        }
        
        public static void Draw()
        {    
             VertsCount = 0;
            if(MainBuffer.Enabled)
            {
                SetupDrawing();
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
            MainBuffer.Dispose();
            UIRenderer.Dispose();
        }
    }
}