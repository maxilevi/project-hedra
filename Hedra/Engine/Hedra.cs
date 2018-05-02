#if DEBUG
    #define SHOW_COLLISION
#endif

using Hedra.Engine.EnvironmentSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Networking;
using Hedra.Engine.Player;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Effects;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Scenes;
using Hedra.Engine.Sound;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using Hedra.Engine;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.PhysicsSystem;
using Forms = System.Windows.Forms;

namespace Hedra
{
    class Hedra : GameWindow
	{ 
	
		private GUITexture _studioLogo, _studioBackground;
		private Panel _debugPanel;
		private GUIText _positionText;
		private GUIText _meshQueueCount;
		private GUIText _generationQueueCount;
		private GUIText _chunkText;
		private GUIText _renderText;
		private GUIText _meshesText;
		private GUIText _cameraText;
	    private Texture _geomPoolMemory;
        private Chunk _underChunk;
		private bool _finishedLoading;
	    private float _splashOpacity = 1;
        private float _lastValue = float.MinValue;
	    private float _passedTime;
        public string GameVersion;
		public bool FirstLaunch;
	    public static int MainThreadId;
	    public float VRam = 0;
		
		public Hedra(int Width, int Height) : base( Width, Height){}
		public Hedra(int Width, int Height, GraphicsMode Mode) : base( Width, Height, Mode){}
		public Hedra(int Width, int Height, GraphicsMode Mode, string Title) : base( Width, Height, Mode, Title){}
		public Hedra(int Width, int Height, GraphicsMode Mode, string Title, GameWindowFlags Options, DisplayDevice Device)
			: base( Width, Height, Mode, Title, Options, Device){}

		protected override void OnLoad(EventArgs e){
			base.OnLoad(e);
		    string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/";
		    string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/" + "Project Hedra/";
            this.GameVersion = "α 0.29";
		    this.Title += " "+GameVersion;
            Hedra.MainThreadId = Thread.CurrentThread.ManagedThreadId;

            OSManager.Load(Assembly.GetExecutingAssembly().Location);
		    AssetManager.Load();
            CompatibilityManager.Load(appPath);

            Log.WriteLine("OS = " + Environment.OSVersion + Environment.NewLine +
		                  "CPU = " + OSManager.CPUArchitecture + Environment.NewLine +
		                  "Graphics Card = " + OSManager.GraphicsCard + Environment.NewLine);

            GameLoader.LoadArchitectureSpecificFiles(appPath);

		    if (!File.Exists(appData + "settings.cfg"))
		        FirstLaunch = true;
            
            GameLoader.CreateCharacterFolders(appData, appPath);

            _studioLogo = new GUITexture(Graphics2D.LoadFromAssets("Assets/splash-logo.png"), Graphics2D.SizeFromAssets("Assets/splash-logo.png"), Vector2.Zero);
			_studioLogo.Enabled = true;
		    _studioLogo.Opacity = 0;

            _studioBackground = new GUITexture(Graphics2D.LoadFromAssets("Assets/splash-background.png"),  Vector2.One, Vector2.Zero);
			_studioBackground.Enabled = true;
		    _studioBackground.Opacity = 0;

            GameLoader.AllocateMemory();
			NameGenerator.Load();		
			CacheManager.Load();
			Physics.Manager.Load();
			Log.WriteLine("Assets loading was Successful.");

		    GameLoader.LoadSoundEngine();

            GameSettings.VSync = true;
			GameSettings.Load(appData + "settings.cfg");
			Log.WriteLine("Settings loading was Successful");

            GameManager.Load();
			Log.WriteLine("Scene loading was Successful.");
			Log.WriteLine("Supported GLSL version is : "+GL.GetString(StringName.ShadingLanguageVersion));

			_debugPanel = new Panel();
			
			_positionText = new GUIText("", new Vector2(.65f,-.9f), Color.Black, FontCache.Get(UserInterface.Fonts.Families[0],12));
			_meshQueueCount = new GUIText("", new Vector2(.65f,-.8f), Color.Black, FontCache.Get(UserInterface.Fonts.Families[0],12));                      
			_generationQueueCount = new GUIText("", new Vector2(.65f,-.7f), Color.Black, FontCache.Get(UserInterface.Fonts.Families[0],12));
			_chunkText = new GUIText("", new Vector2(.65f,-.6f), Color.Black, FontCache.Get(UserInterface.Fonts.Families[0],12));
			_renderText = new GUIText("", new Vector2(.65f,-.5f), Color.Black, FontCache.Get(UserInterface.Fonts.Families[0],12));			
			_meshesText = new GUIText("", new Vector2(.65f,-.4f), Color.Black, FontCache.Get(UserInterface.Fonts.Families[0],12));
			_cameraText = new GUIText("", new Vector2(.65f,-.3f), Color.Black, FontCache.Get(UserInterface.Fonts.Families[0],12));
			_geomPoolMemory = new Texture(0, new Vector2(0f, 0.95f), new Vector2(1024f / GameSettings.Width, 16f / GameSettings.Height));
            /*Texture WaterTexture = new Texture(WaterEffects.WaterFBO.TextureID[0], Vector2.Zero, Vector2.One);
			WaterTexture.TextureElement.Flipped = true;
			
			DebugPanel.AddElement(WaterTexture);*/
			_debugPanel.AddElement(_positionText);
			_debugPanel.AddElement(_chunkText);
			_debugPanel.AddElement(_renderText);
			_debugPanel.AddElement(_generationQueueCount);
			_debugPanel.AddElement(_meshQueueCount);
			_debugPanel.AddElement(_meshesText);
			_debugPanel.AddElement(_cameraText);
            _debugPanel.AddElement(_geomPoolMemory);
			_debugPanel.Disable();
			
			GL.BlendEquation(BlendEquationMode.FuncAdd);
	        GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
	        GraphicsLayer.Enable(EnableCap.Texture2D);
	        
	        string GLVersion = GL.GetString(StringName.Version);
            Log.WriteLine(GLVersion);

            float ShadingOpenGLVersion = this.GetShadingVersion(GLVersion);

			if( ShadingOpenGLVersion < 3.1f){//OpenGL Core
				Forms.MessageBox.Show("Minimum OpenGL version is 3.1, yours is "+ShadingOpenGLVersion, "OpenGL Version not supported",
				                      Forms.MessageBoxButtons.OK, Forms.MessageBoxIcon.Error);
				this.Exit();
			}

            //if (ShadingOpenGLVersion > 4.1f && FirstLaunch)
            //    GraphicsOptions.SSAO = true;
#if !DEBUG
            TaskManager.Delay(6000, () => _splashOpacity = 0);
#endif
#if DEBUG
            this._finishedLoading = true;
#endif
#if DEBUG
            GraphicsLayer.Enable(EnableCap.DebugOutput);
            GL.DebugMessageCallback(
                delegate(DebugSource Source, DebugType Type, int Id, DebugSeverity Severity, int Length, IntPtr Message,
                    IntPtr Param)
                {
                    if(Type != DebugType.DebugTypeError) return;
                    Log.WriteLine(Source);
                    Log.WriteLine(Marshal.PtrToStringAnsi(Message));
                    Log.WriteLine(Severity);
                    Log.WriteLine(Marshal.PtrToStringAnsi(Param));
                }, IntPtr.Zero);
#endif
        }

	    protected override void OnUpdateFrame(FrameEventArgs e){
			base.OnUpdateFrame(e);

			Utils.CalculateFrameRate();

            Time.timeScale = GameSettings.Paused ? 0 : 1;
			Time.deltaTime = e.Time * Time.timeScale;
			Time.unScaledDeltaTime = (float) e.Time;
			Time.FrameTimeSeconds = (float) e.Time;//Utils.FrameProccesingTime / 1000;
			Time.ScaledFrameTimeSeconds =  Time.FrameTimeSeconds * (float) Time.timeScale;
			Time.CurrentFrame += (float) Time.deltaTime;
			Time.UnPausedCurrentFrame += Time.FrameTimeSeconds;
			
			AnalyticsManager.AverageFPS += Utils.LastFrameRate;
			AnalyticsManager.AverageFPS *= .5f;
			AnalyticsManager.PlayTime += Time.FrameTimeSeconds;
			
			CoroutineManager.Update();
			ThreadManager.Update();
            UpdateManager.Update();
	        Physics.Manager.Update();
            SoundManager.Update(LocalPlayer.Instance.Position);
			SoundtrackManager.Update();
			AutosaveManager.Update();

	        if (!this._finishedLoading)
	        {
	            _studioBackground.Opacity = Mathf.Lerp(_studioBackground.Opacity, _splashOpacity, Time.unScaledDeltaTime);
                _studioLogo.Opacity = Mathf.Lerp(_studioLogo.Opacity, _splashOpacity, Time.unScaledDeltaTime);

	            if (_splashOpacity == 0 && Math.Abs(_studioLogo.Opacity - _splashOpacity) < 0.05f)
	            {
	                this._finishedLoading = true;
	            }
	        }
	        //Utils.RNG is not thread safe so it might break itself
            //Here is the autofix because thread-safety is for loosers
            float newNumber = Utils.Rng.NextFloat();
			if(newNumber == 0 && _lastValue == 0){
				Random Rng = Utils.Rng;
				Utils.Rng = new Random();//Reset it
				_lastValue = float.MinValue;
			}else if(_lastValue != 0)
				_lastValue = newNumber;
			
			
			LocalPlayer Player = GameManager.Player;
			DrawManager.FrustumObject.SetFrustum(GameManager.Player.View.ModelViewMatrix);
			Vector2 Vec2 = World.ToChunkSpace(Player.Position);
			//Log.WriteLine( (System.GC.GetTotalMemory(false) / 1024 / 1024) + " MB");
			if(GameSettings.Debug){
				
				Chunk UChunk = World.GetChunkAt(Player.Position);
				
				int MobCount = World.Entities.Count;
				for(var i = 0; i < World.Entities.Count; i++){
					//if(World.Entities[i] != null && !World.Entities[i].IsStatic)
					//	MobCount++;
				}
				_underChunk = World.GetChunkByOffset(Vec2);
				_debugPanel.Enable();
				_positionText.Text = "X = "+(int)Player.BlockPosition.X+" Y = "+(int)(Player.BlockPosition.Y)+" Z = "+(int)Player.BlockPosition.Z;
				if(_underChunk != null)
					_chunkText.Text = "Chunks = "+ World.Chunks.Count+" ChunkX = "+_underChunk.OffsetX+" ChunkZ = "+_underChunk.OffsetZ;
				_meshesText.Text = " Lights = "+ShaderManager.UsedLights +" / " +ShaderManager.MaxLights + " Pitch = "+Player.View.Pitch;
				_meshQueueCount.Text = "Mesh Queue = "+ World.MeshQueue.Queue.Count + 
					"Cache ="+CacheManager.CachedColors.Count + " | "+CacheManager.CachedExtradata.Count + " Time = "+(int)(SkyManager.DayTime/1000)+":"+((int) ( ( SkyManager.DayTime/1000f - (int)(SkyManager.DayTime/1000) ) * 60)).ToString("00");
				_generationQueueCount.Text =  "Generation Queue ="+ World.ChunkGenerationQueue.Queue.Count+" Mobs = "+MobCount +" Yaw = "+Player.View.TargetYaw;
				_renderText.Text = "Textures = "+Graphics2D.Textures.Count+" Seed= "+ World.Seed + " FPS= "+Utils.LastFrameRate + " MS="+Utils.FrameProccesingTime;
				_cameraText.Text = "Pitch = "+Player.View.TargetPitch+" Physics Calls = "+ Physics.Manager.Count;
                
			    _passedTime += Time.FrameTimeSeconds;
			    if (_passedTime > 5.0f)
			    {
			        _passedTime = 0;
			        Graphics2D.Textures.Remove(_geomPoolMemory.TextureElement.TextureId);
                    GL.DeleteTexture(_geomPoolMemory.TextureElement.TextureId);
			        _geomPoolMemory.TextureElement.TextureId = Graphics2D.LoadTexture(WorldRenderer.StaticBuffer.Indices.Draw());
			    }
			}
            else
				_debugPanel.Disable();
			
			
		}

		protected override void OnRenderFrame(FrameEventArgs e){
           
			base.OnRenderFrame(e);

			GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);


		    if (!this._finishedLoading)
		    {
                DrawManager.UIRenderer.Draw(_studioBackground);
		        DrawManager.UIRenderer.Draw(_studioLogo);
		    }
		    else
		    {
		        DrawManager.Draw();
            }

#if SHOW_COLLISION
			            if(GameSettings.Debug){
			           
				            LocalPlayer Player = GameManager.Player;
				            Chunk UnderChunk = World.GetChunkAt(Player.Position);
                /*
                 if(UnderChunk != null){
                     for(int x = 0; x < Chunk.ChunkWidth / Chunk.BlockSize; x++){
                         for(int z = 0; z < Chunk.ChunkWidth / Chunk.BlockSize; z++){
                             Vector3 BasePosition = new Vector3(x*Chunk.BlockSize+UnderChunk.OffsetX, Physics.HeightAtPosition(x*Chunk.BlockSize + UnderChunk.OffsetX, z*Chunk.BlockSize + UnderChunk.OffsetZ), z*Chunk.BlockSize + UnderChunk.OffsetZ);
                             Vector3 Normal = Physics.NormalAtPosition(BasePosition);

                             GL.Begin(PrimitiveType.Lines);
                             GL.Color3(Color.Yellow);
                             GL.Vertex3(BasePosition);
                             GL.Color3(Color.Yellow);
                             GL.Vertex3(BasePosition + Normal * 2);
                             GL.End();
                         }
                     }
                 }*/

			    GL.Begin(PrimitiveType.Lines);
			    GL.Color3(Color.Blue);
			    GL.Vertex3(Player.Position + Vector3.UnitZ * 2f);
			    GL.Color3(Color.Blue);
			    GL.Vertex3(Player.Position + Vector3.UnitZ * 4f);
			    GL.End();

			    GL.Begin(PrimitiveType.Lines);
			    GL.Color3(Color.BlueViolet);
			    GL.Vertex3(Player.Position - Vector3.UnitZ * 2f);
			    GL.Color3(Color.BlueViolet);
			    GL.Vertex3(Player.Position - Vector3.UnitZ * 4f);
			    GL.End();

                GL.Begin(PrimitiveType.Lines);
			    GL.Color3(Color.Red);
			    GL.Vertex3(Player.Position + Vector3.UnitX * 2f);
			    GL.Color3(Color.Red);
			    GL.Vertex3(Player.Position + Vector3.UnitX * 4f);
			    GL.End();

			    GL.Begin(PrimitiveType.Lines);
			    GL.Color3(Color.OrangeRed);
			    GL.Vertex3(Player.Position - Vector3.UnitX * 2f);
			    GL.Color3(Color.OrangeRed);
			    GL.Vertex3(Player.Position - Vector3.UnitX * 4f);
			    GL.End();

			    GL.Begin(PrimitiveType.Lines);
			    GL.Color3(Color.Yellow);
			    GL.Vertex3(Player.Position + Player.Orientation * 2f);
			    GL.Color3(Color.Yellow);
			    GL.Vertex3(Player.Position + Player.Orientation * 4f);
			    GL.End();

                World.Entities.ToList().ForEach( delegate(Entity E) 
                {
                    if (E != null)
                    {
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Line);
                        BasicGeometry.DrawBox(E.HitBox.Min, E.BaseBox.Max - E.BaseBox.Min);
                        GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
                    }
                });

                //LocalPlayer Player = Game.LPlayer;
                var Collisions = new List<ICollidable>();
			                var Collisions2 = new List<ICollidable>();
				
				            //Chunk UnderChunk = World.GetChunkAt(Player.BlockPosition);
				            Chunk UnderChunkR = World.GetChunkAt(Player.Position + new Vector3(Chunk.Width,0, 0));
				            Chunk UnderChunkL = World.GetChunkAt(Player.Position - new Vector3(Chunk.Width, 0, 0));
				            Chunk UnderChunkF = World.GetChunkAt(Player.Position + new Vector3(0,0,Chunk.Width));
				            Chunk UnderChunkB = World.GetChunkAt(Player.Position - new Vector3(0,0,Chunk.Width));

				            Collisions.AddRange(World.GlobalColliders);
                            if(Player.NearCollisions != null)
                                Collisions.AddRange(Player.NearCollisions);
				            if(UnderChunk != null)
					            Collisions.AddRange(UnderChunk.CollisionShapes);
				            if(UnderChunkL != null)
					            Collisions2.AddRange(UnderChunkL.CollisionShapes);
				            if(UnderChunkR != null)
					            Collisions2.AddRange(UnderChunkR.CollisionShapes);
				            if(UnderChunkF != null)
					            Collisions2.AddRange(UnderChunkF.CollisionShapes);
				            if(UnderChunkB != null)
					            Collisions2.AddRange(UnderChunkB.CollisionShapes);
				
				            for(int i = 0; i < Collisions.Count; i++)
				            {
				                var shape = Collisions[i] as CollisionShape;
                                if(shape == null) return;
                                
				                var pshape = Player.HitBox.ToShape();

                                float radiiSum = shape.BroadphaseRadius + pshape.BroadphaseRadius;

				                BasicGeometry.DrawShape(shape, (pshape.Center - shape.Center).LengthSquared < radiiSum * radiiSum ? Color.White : Color.Red);
				            }
				
				            for(int i = 0; i < Collisions2.Count; i++)
				            {
				                /*var shape = Collisions2[i] as CollisionShape;
				                if( shape != null){
				                    BasicGeometry.DrawShape(shape, Color.Yellow);
					            }*/
				            }
			            }
#endif

            this.SwapBuffers();
			
		}

	    private bool _forcingResize;
        protected override void OnResize(EventArgs e)
	    {
            base.OnResize(e);
	        if (_forcingResize) return;

	        _forcingResize = true;
	        this.Width = GameSettings.Width;
	        this.Height = GameSettings.Height;
	        _forcingResize = false;


	        //DrawManager.UIRenderer.RescaleTextures(this.Width, this.Height);
            //GameSettings.Width = Width;
            //GameSettings.Height = Height;

            //MainFBO.DefaultBuffer.Resize();
        }

        public void UpdateTextures(int Width, int Height){
			//DrawManager.UIRenderer.RescaleTextures(Width, Height);
			GameSettings.Width = Width;
			GameSettings.Height = Height;
			
			DrawManager.FrustumObject.SetFrustum(GameManager.Player.View.ModelViewMatrix);
			DrawManager.FrustumObject.CalculateFrustum(DrawManager.FrustumObject.ProjectionMatrix, GameManager.Player.View.ModelViewMatrix);
			//Resize FBOs
			MainFBO.DefaultBuffer.Resize();
			//Game.LPlayer.Inventory.Resize();
			
			GameManager.Player.UI = new UserInterface(GameManager.Player);
		}
		
		protected override void OnFocusedChanged(EventArgs e)
		{
			base.OnFocusedChanged(e);
			if(!this.Focused){
				if(!GameManager.InStartMenu && !GameManager.IsLoading && !GameSettings.Paused &&
                    GameManager.Player != null && !GameManager.Player.Inventory.Show &&
                    !GameManager.Player.AbilityTree.Show && !GameManager.Player.Trade.Show)
                {
					//GameSettings.Paused = true;
					GameManager.Player.UI.ShowMenu();
				}
			}
		}
		
		protected override void OnUnload(EventArgs e)
		{
		    AssetManager.Dispose();
			GameSettings.Save(AssetManager.AppData + "settings.cfg");
			
			if(!GameManager.InStartMenu) AutosaveManager.Save();
			if(NetworkManager.IsConnected) NetworkManager.Exit();
			DisposeManager.DisposeAll();
			Graphics2D.Dispose();
			base.OnUnload(e);
#if !DEBUG
			//AnalyticsManager.SendData();
#endif
			//Environment.Exit(Environment.ExitCode);
			//base.OnUnload(e);
		}

	    private float GetShadingVersion(string GLVersion)
	    {
	        GLVersion = GLVersion.Replace(",", ".");
            Regex ShadingReg = new Regex(@"([0-9]+\.[0-9]+|[0-9]+)");
	        Match ShadingStringVersion = ShadingReg.Match(GLVersion);
	        float ShadingOpenGLVersion = float.Parse(ShadingStringVersion.Value, CultureInfo.InvariantCulture);
	        return (ShadingOpenGLVersion >= 310) ? ShadingOpenGLVersion / 100f : ShadingOpenGLVersion;

	    }
    }
}

