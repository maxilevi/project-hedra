/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 25/01/2016
 * Time: 09:49 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK.Input;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using OpenTK;
using System.Reflection;
using OpenTK.Graphics.OpenGL;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.EntitySystem;

namespace Hedra.Engine.Rendering.UI
{
	public class UserInterface
	{
		private readonly LocalPlayer _player;
		public static PrivateFontCollection Fonts = new PrivateFontCollection();
		public const int Sans = 0;
		public bool ShowHelp = false;
		public Panel Menu;
		public OptionsUI OptionsMenu;
		public GameUI GamePanel;
		public ChrChooserUI ChrChooser;
		public ChrCreatorUI ChrCreator;
		public NetworkUI ConnectPanel;
		private Texture _title;
		private Button _newRun;
		
		public static FBO PlayerFbo;
		public static FBO InventoryFbo;
	    public static FBO QuestFbo;
	    public static Font Regular;
		public static Color DefaultFontColor = Color.White;
		
		public UserInterface (LocalPlayer Player){
			this._player = Player;
			PlayerFbo = new FBO(GameSettings.Width, GameSettings.Height);
			InventoryFbo = new FBO(GameSettings.Width, GameSettings.Height);
			QuestFbo = new FBO(GameSettings.Width, GameSettings.Height);

			byte[] sansRegular = AssetManager.ReadBinary("Assets/ClearSans-Regular.ttf", AssetManager.DataFile3);          	
			Fonts.AddMemoryFont(Utils.IntPtrFromByteArray(sansRegular), sansRegular.Length);
			
			Menu = new Panel();
			OptionsMenu = new OptionsUI();
			GamePanel = new GameUI(Player);
			ChrChooser = new ChrChooserUI(Player);
			ConnectPanel = new NetworkUI();
			ChrCreator = new ChrCreatorUI(Player);

			Vector2 bandPosition = new Vector2(0, -.8f);
			
			_title = new Texture(Graphics2D.LoadFromAssets("Assets/UI/Menu.png"),
			                       Vector2.Zero, Vector2.One);
			
			/*Alpha = new Texture(Graphics2D.LoadFromAssets("Assets/UI/alpha.png"),
			                     new Vector2(-.25f, .4f), Graphics2D.SizeFromAssets("Assets/UI/alpha.png") * .6f);*/
			
			Texture blackBand = new Texture(Color.FromArgb(0,69,69,69), Color.FromArgb(255,19,19,19), bandPosition, new Vector2(1f, 0.08f / GameSettings.Height * 578), GradientType.LeftRight);
			
			
			_newRun = new Button(new Vector2(.1f, bandPosition.Y),
			                    new Vector2(0.15f,0.075f), "New World", 0, DefaultFontColor, FontCache.Get(Fonts.Families[Sans], 16));
			
			_newRun.Click += new OnButtonClickEventHandler(NewRunOnClick);
			
			Button chooseChr = new Button(new Vector2(.3f, bandPosition.Y),
			                             new Vector2(0.15f,0.075f), "Load World", 0, DefaultFontColor, FontCache.Get(Fonts.Families[Sans], 16));

			chooseChr.Click += delegate {
				if(!GameManager.InStartMenu){
					AutosaveManager.Save();
					GameManager.LoadMenu();
				}
				Menu.Disable();
                ChrChooser.Enable();
			};
			
			Button connectToServer = new Button(new Vector2(.535f, bandPosition.Y),
			                             new Vector2(0.15f,0.075f), "Multiplayer", 0, DefaultFontColor, FontCache.Get(Fonts.Families[Sans], 16));
			
			Button disconnect = new Button(new Vector2(.535f, bandPosition.Y),
			                             new Vector2(0.15f,0.075f), "Disconnect", 0, DefaultFontColor, FontCache.Get(Fonts.Families[Sans], 16));
			disconnect.Click += delegate{ Networking.NetworkManager.Disconnect(true); };
			
			connectToServer.Click += delegate{
			    Player.MessageDispatcher.ShowNotification("Multiplayer is down.", Color.DarkRed, 3f, true);
			};
			
			Button options = new Button(new Vector2(.75f, bandPosition.Y),
			                            new Vector2(0.15f,0.075f), "Options", 0, DefaultFontColor, FontCache.Get(Fonts.Families[Sans], 16));
			
			options.Click += delegate(object Sender, MouseButtonEventArgs E) { Menu.Disable(); OptionsMenu.Enable();};
			
			Button quit = new Button(new Vector2(.9f, bandPosition.Y),
			                         new Vector2(0.15f,0.075f), "Exit", 0, DefaultFontColor, FontCache.Get(Fonts.Families[Sans], 16));
			
			quit.Click += delegate { Program.GameWindow.Exit(); };
			
			if( Program.GameWindow.GameVersion != "Unknown" ){
				GUIText versionText = new GUIText(Program.GameWindow.GameVersion, Vector2.Zero, Color.Black, FontCache.Get(Fonts.Families[Sans], 8));
				versionText.Position = new Vector2(-1,1) + new Vector2(versionText.Scale.X, -versionText.Scale.Y);
				Menu.AddElement(versionText);
			}
			
			Menu.AddElement(blackBand);
			Menu.AddElement(connectToServer);
			Menu.AddElement(_title);
			Menu.AddElement(quit);
			Menu.AddElement(_newRun);
			Menu.AddElement(chooseChr);
			Menu.AddElement(options);
			Menu.AddElement(disconnect);
			//Menu.AddElement(Alpha);
			
			Menu.OnPanelStateChange += delegate(object Sender, PanelState E) { 
				if(E == PanelState.Enabled){
					if(Networking.NetworkManager.IsConnected){
						_newRun.Disable();
						chooseChr.Disable();
						connectToServer.Disable();
						disconnect.Enable();
					}else{
						disconnect.Disable();
					}
				}
			};

		}
		
		public void NewRunOnClick(object Sender, EventArgs E){
			if(GameManager.InStartMenu){
				Menu.Disable();
				ChrChooser.Enable();
			}else{
				GameManager.NewRun(_player);
			}
		}
		

		public void Draw(){
			if(!GameSettings.Paused && GameSettings.Debug)
            	DrawCoordinateSystem();
		}
		
		public void Update(){
			if(_player != null){
				if(this.GamePanel.Enabled){
					if(Program.GameWindow.FirstLaunch){
						Program.GameWindow.FirstLaunch = false;
					    _player.MessageDispatcher.ShowMessageWhile("[F4] HELP", delegate { return !LocalPlayer.Instance.UI.ShowHelp; });
					}
				}

				this.GamePanel.Compass.Disable();
				this.GamePanel.Compass.TextureElement.Angle = _player.Model.Model.Rotation.Y;
				
				if(this.ShowHelp && this.GamePanel.Enabled){
					_player.AbilityTree.Show = false;
					_player.QuestLog.Show = false;
					GamePanel.Help.Enable();
				}else{
					GamePanel.Help.Disable();
				}				
				this.GamePanel.ClassLogo.BaseTexture.TextureElement.TextureId = _player.Class.Logo;
			}
		}

		private float _oldRotation;
		 public void DrawPreview(Model Model, FBO Fbo){
            if (Model == null)
                return;
            bool prevEnabled = Model.Enabled;
            Model.Enabled = true;
            GL.Enable(EnableCap.DepthTest);
            int prevShader = GraphicsLayer.ShaderBound;
            int prevFbo = GraphicsLayer.FBOBound;
            Fbo.Bind();

            GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);

            Vector3 oldRot = Model.Rotation;
            bool usedFog = (Model as QuadrupedModel)?.Fog ?? ((Model as HumanModel)?.Fog ?? false);

            _oldRotation += 25 * (float)Time.unScaledDeltaTime / DataManager.CharacterCount;
            if (Model is QuadrupedModel)
                (Model as QuadrupedModel).Fog = false;

            if (Model is HumanModel)
                (Model as HumanModel).Fog = false;

            Matrix4 projectionMatrix = Matrix4.CreatePerspectiveFieldOfView((_player.Model == Model ? 50 : 60) * Mathf.Radian, 1.5f, 1, 1024);
            Matrix4 rotationMatrix = Matrix4.CreateRotationY(_oldRotation * Mathf.Radian);
            Matrix4 lookAt = Matrix4.LookAt(Vector3.TransformPosition(-Vector3.UnitZ * ((Model == _player.Model) ? 180f : 30f), rotationMatrix) + Model.Position,
                                            Vector3.TransformPosition(Vector3.UnitY * ((Model == _player.Model) ? 41f : 15f), rotationMatrix) + Model.Position, Vector3.UnitY);


            if (Model is QuadrupedModel)
            {
                (Model as QuadrupedModel).Model.DrawModel(lookAt * projectionMatrix, lookAt);
            }
            else if (Model is HumanModel)
            {


                (Model as HumanModel).Model.DrawModel(lookAt * projectionMatrix, lookAt);
                /*if((Model as HumanModel).LeftWeapon.Meshes != null){
                    for(int i = 0; i < (Model as HumanModel).LeftWeapon.Meshes.Length; i++){
                        Vector3 Prevrot = (Model as HumanModel).LeftWeapon.Meshes[i].Rotation;
                        //(Model as HumanModel).LeftWeapon.Meshes[i].Rotation = Vector3.UnitY * OldRotation;
                        //(Model as HumanModel).LeftWeapon.Meshes[i].Draw();
                        //Model as HumanModel).LeftWeapon.Meshes[i].Rotation = Prevrot;
                    }
                }*/
            }

            if (Model is QuadrupedModel)
                (Model as QuadrupedModel).Fog = usedFog;

            if (Model is HumanModel)
                (Model as HumanModel).Fog = usedFog;

            GL.Disable(EnableCap.DepthTest);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, prevFbo);
            GraphicsLayer.FBOBound = prevFbo;
            Model.Enabled = prevEnabled;
            GL.UseProgram(prevShader);
            DrawManager.FrustumObject.CalculateFrustum(DrawManager.FrustumObject.ProjectionMatrix, DrawManager.FrustumObject.ModelViewMatrix);
		     GraphicsLayer.ShaderBound = prevShader;
            GL.Enable(EnableCap.Blend);
        }
		
		public void DrawPreview(ObjectMesh Mesh, FBO Fbo){
			if(	Mesh == null)
				return;
			bool prevEnabled  = Mesh.Enabled;
			Mesh.Enabled = true;
			GL.Enable(EnableCap.DepthTest);
			int prevShader = GraphicsLayer.ShaderBound;
			int prevFbo = GraphicsLayer.FBOBound;
			Fbo.Bind();
				GL.ClearColor(Color.FromArgb(0,0,0,0));
				GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
				
				Vector3 oldRot = Mesh.Rotation;
				bool usedFog = Mesh.ApplyFog;
				Vector2 posScale = Mathf.ScaleGUI(new Vector2(1024,578), new Vector2(1,1));
				
				_oldRotation += 25 * (float) Time.unScaledDeltaTime * 0.33f;
				Mesh.Rotation = new Vector3(Mesh.Rotation.X, _oldRotation, Mesh.Rotation.Z);
				Mesh.ApplyFog = false;
				Vector3 oldPosition = Mesh.Position;
				Mesh.Position = Vector3.Zero;
				
				Matrix4 projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(50 * Mathf.Radian, 1.33f, 1, 1024);
				GL.MatrixMode(MatrixMode.Projection);
				GL.LoadMatrix(ref projectionMatrix);
				
				Matrix4 lookAt = Matrix4.LookAt(Vector3.UnitZ * 10f, Vector3.UnitY * 4f, Vector3.UnitY);
				GL.MatrixMode(MatrixMode.Modelview);
				GL.LoadMatrix(ref lookAt);
				
				Mesh.Draw();
				
				Mesh.ApplyFog = usedFog;
				Mesh.Rotation = oldRot;
				Mesh.Position = oldPosition;
				
				_player.View.RebuildMatrix();
				DrawManager.FrustumObject.SetFrustum(_player.View.Matrix);
				
			GL.Disable(EnableCap.DepthTest);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, prevFbo);
			GraphicsLayer.FBOBound = prevFbo;
			Mesh.Enabled = prevEnabled;
			GL.UseProgram(prevShader);
			GraphicsLayer.ShaderBound = prevShader;
			GL.Enable(EnableCap.Blend);
		}
		
		private void DrawCoordinateSystem(){
           /* GL.PushMatrix();
            GL.Translate(Player.View.LookAtPoint + new Vector3(Player.Position.X, Player.Position.Y, Player.Position.Z));
            
            GL.Begin(PrimitiveType.Lines);
            
            GL.Color3(System.Drawing.Color.Red);
            GL.Vertex3(0,0,0);
            GL.Color3(System.Drawing.Color.Red);
            GL.Vertex3(0.1f,0,0);
            
            GL.Color3(System.Drawing.Color.Yellow);
            GL.Vertex3(0,0,0);
            GL.Color3(System.Drawing.Color.Yellow);
            GL.Vertex3(0,0.1f,0);
            
            GL.Color3(System.Drawing.Color.Blue);
            GL.Vertex3(0,0,0);
            GL.Color3(System.Drawing.Color.Blue);
            GL.Vertex3(0,0,0.1f);
            GL.Color3(System.Drawing.Color.Transparent);
            
            GL.End();
            
            GL.PopMatrix();*/
		}
		
		public void ShowMenu(){
			if(GameManager.IsLoading || GameManager.InMenu) return;
			
			Menu.Enable();
			OptionsMenu.Disable();
			ChrChooser.Disable();
			if(_player?.Inventory != null) _player.Inventory.Show = false;
			_player.AbilityTree.Show = false;
			GamePanel.Disable();
			ChrCreator.Disable();
			ConnectPanel.Disable();
			if(!Networking.NetworkManager.IsConnected){
				GameSettings.Paused = true;
			}else{
				_player.View.LockMouse = false;
				_player.Movement.Check = false;
				_player.View.Check = false;
			}
			UpdateManager.CursorShown = true;
			LocalPlayer.Instance.Chat.Show = false;
			GameSettings.DarkEffect = false;
			System.Windows.Forms.Cursor.Position = new System.Drawing.Point(GameSettings.Width / 2, GameSettings.Height/2);
			//CoroutineManager.StartCoroutine(MenuEnter);
		}
			
		public void HideMenu(){
			//GraphicsOptions.DarkEffect = false;
			GameSettings.Paused = false;
			if(Networking.NetworkManager.IsConnected){
				_player.View.LockMouse = true;
				_player.Movement.Check = true;
				_player.View.Check = true;
			}
			Menu.Disable();
			OptionsMenu.Disable();
			GamePanel.Enable();
			ChrChooser.Disable();
			ChrCreator.Disable();
			ConnectPanel.Disable();
			UpdateManager.CursorShown = false;
			LocalPlayer.Instance.Chat.Show = true;
			LocalPlayer.Instance.Chat.LoseFocus();
			System.Windows.Forms.Cursor.Position = new System.Drawing.Point(GameSettings.Width / 2, GameSettings.Height/2);
			//Menu.Move(new Vector2(2,0));
		}
		
		private IEnumerator MenuEnter(){
			while(Menu.Position.X > 0f){
				Menu.Move(new Vector2(-0.025f,0));
				yield return null;
			}
			Menu.MoveTo(Vector2.Zero);
		}
		
		private List<bool> _wasEnabled = new List<bool>();
		private bool _mEnabled;
		public bool Hide{
			get{ return _mEnabled; }
			set{
				BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
				int k = 0 ;
				for(int i = 0; i < this.GetType().GetFields(flags).Length; i++)
				{
					FieldInfo field = this.GetType().GetFields(flags)[i];
					if(typeof(Panel).IsAssignableFrom(field.FieldType)){
						if(value){
							_wasEnabled.Add((field.GetValue(this) as Panel).Enabled);
							(field.GetValue(this) as Panel).Disable();
						}else{
							if(!_mEnabled)
								return;
							if(_wasEnabled[k])
								(field.GetValue(this) as Panel).Enable();
						}
						k++;
					}
				}
				if(!value)
					_wasEnabled.Clear();
				_mEnabled = value;
			}
		}
		
		
		
	}
}
