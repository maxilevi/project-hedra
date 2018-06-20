/*
 * Author: Zaphyk
 * Date: 04/02/2016
 * Time: 05:35 p.m.
 *
 */
using System.Collections.Generic;
using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Effects;
using Hedra.Engine.Rendering.UI;
using OpenTK.Graphics.OpenGL;

namespace Hedra.Engine.Management
{
	/// <summary>
	/// A static class which acts as a layer between every Draw() method and the actual rendeirng 
	/// </summary>
	public static class DrawManager
	{
		private static readonly List<IRenderable> DrawFunctions = new List<IRenderable>();
		public static List<IRenderable> ParticleRenderer = new List<IRenderable>();
	    public static List<IRenderable> TrailRenderer = new List<IRenderable>();
        public static MainFBO MainBuffer;
		public static GUIRenderer UIRenderer = new GUIRenderer();
		public static FrustumCulling FrustumObject = new FrustumCulling();
		public static DropShadowRenderer DropShadows = new DropShadowRenderer();
		public static Cursor MouseCursor;
		public static int CullableObjectsCount;
		public static int CulledObjectsCount;
		public static int DrawCalls;
		public static int VertsCount;
	    private static bool _initialized;

        public static void Add(IRenderable a)
	     {
			
			lock(DrawFunctions)
				DrawFunctions.Add(a);
			
			CullableObjectsCount = 0;
			lock (DrawFunctions)
			{
			    for(int i = 0; i < DrawFunctions.Count; i++){
			        if(DrawFunctions[i] is ICullable)
			            CullableObjectsCount++;
			    }
			}
	     }
		
		public static void Remove(IRenderable a){
			lock(DrawFunctions)
				DrawFunctions.Remove(a);
		}
		
		public static bool Contains(IRenderable a){
			return DrawFunctions.Contains(a);
		}
		
		public static void BulkDraw(){
			SkyManager.Draw();
			World.CullTest(FrustumObject);
			World.Draw(ChunkBufferTypes.STATIC);
			DropShadows.Draw();
			
	    	IRenderable[] draws;
	    	lock(DrawFunctions)
	    		draws = DrawFunctions.ToArray();
	    	
	    	var drawedObjects = 0;
	    	var drawedCullableObjects = 0;
			for(var i = 0; i < draws.Length; i++)
			{
			    if (draws[i] == null) continue;

			    if (draws[i] is ICullable cullable)
			    {
			        if (!FrustumObject.IsInsideFrustum(cullable)) continue;
			        draws[i].Draw();
			        drawedObjects++;
			        drawedCullableObjects++;
			    }
			    else
			    {
			        draws[i].Draw();
			        drawedObjects++;
                }
			}
			GraphicsLayer.Enable(EnableCap.DepthTest);
			World.Draw(ChunkBufferTypes.WATER);
		    for (int i = TrailRenderer.Count - 1; i > -1; i--)
		    {
		        TrailRenderer[i].Draw();
		    }
            for (int i = ParticleRenderer.Count-1; i > -1; i--){
				ParticleRenderer[i].Draw();
			}

			
			var worldCalls = 3;//Water + Static + Shadows
			int dropShadowCalls = DropShadows.Count;//Ideally
			CulledObjectsCount = CullableObjectsCount - drawedCullableObjects;
			DrawCalls = drawedObjects + UIRenderer.DrawCount + worldCalls + dropShadowCalls;
		}
		
	    public static void Draw()
	    {	
	     	VertsCount = 0;
     		if(MainBuffer.Enabled){
    			
    			MainBuffer.CaptureData();
    			BulkDraw();
		    	MainBuffer.UnCaptureData();
    		}
	     	MainBuffer.Draw();
	    	
	    	UIRenderer.Draw();
	    	
	    	MainBuffer.Clear();
	    	
	    	#if DEBUG
	    	if(GameSettings.DebugView){
		    	ErrorCode code = GL.GetError();
		    	if(code != ErrorCode.NoError)
		    		Log.WriteResult(false, "OpenGL error: "+code.ToString());
	    	}
	    	#endif
	    }
	     
	     public static void Load(){
	         MainBuffer = new MainFBO
	         {
	             Enabled = true
	         };
	         _initialized = true;
	     }
	}
}


public enum DrawPriority{
	LOW,
	NORMAL,
	HIGH,
}