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
using System.Reflection;
using Hedra.Engine.Game;
using OpenTK;
using Hedra.Engine.Management;
using Hedra.Engine.Player;

namespace Hedra.Engine.Rendering.UI
{
    public class UserInterface
    {
        private readonly IPlayer _player;
        public bool ShowHelp = false;
        public Panel Menu;
        public OptionsUI OptionsMenu;
        public GameUI GamePanel;
        public ChrChooserUI ChrChooser;
        public ChrCreatorUI ChrCreator;
        public NetworkUI ConnectPanel;
        private Texture _title;
        private Button _newRun;
        private Button _loadButton;
        public static Font Regular;
        public static Color DefaultFontColor = Color.White;
        
        public UserInterface (IPlayer Player)
        {
            this._player = Player;
            
            Menu = new Panel();
            OptionsMenu = new OptionsUI();
            GamePanel = new GameUI(Player);
            ChrChooser = new ChrChooserUI(Player);
            ConnectPanel = new NetworkUI();
            ChrCreator = new ChrCreatorUI(Player);

            var bandPosition = new Vector2(0, -.8f);
            
            _title = new Texture(Graphics2D.LoadFromAssets("Assets/UI/Menu.png"),
                                   new Vector2(-.4f, .35f), Graphics2D.SizeFromAssets("Assets/UI/Menu.png") * .75f);
            
            /*Alpha = new Texture(Graphics2D.LoadFromAssets("Assets/UI/alpha.png"),
                                 new Vector2(-.25f, .4f), Graphics2D.SizeFromAssets("Assets/UI/alpha.png") * .6f);*/
            
            var blackBand = new Texture(Color.FromArgb(0,69,69,69), Color.FromArgb(255,19,19,19), bandPosition, new Vector2(1f, 0.1f / GameSettings.Height * 578), GradientType.LeftRight);
            
            
            _newRun = new Button(new Vector2(.1f, bandPosition.Y),
                                new Vector2(0.15f,0.075f), "New World", 0, DefaultFontColor, FontCache.Get(AssetManager.NormalFamily, 16));
            
            _newRun.Click += new OnButtonClickEventHandler(NewRunOnClick);
            
            _loadButton = new Button(new Vector2(.3f, bandPosition.Y),
                                         new Vector2(0.15f,0.075f), "Load World", 0, DefaultFontColor, FontCache.Get(AssetManager.NormalFamily, 16));

            _loadButton.Click += delegate {
                if(!GameManager.InStartMenu){
                    AutosaveManager.Save();
                    GameManager.LoadMenu();
                }
                Menu.Disable();
                ChrChooser.Enable();
            };
            
            Button connectToServer = new Button(new Vector2(.535f, bandPosition.Y),
                                         new Vector2(0.15f,0.075f), "Multiplayer", 0, DefaultFontColor, FontCache.Get(AssetManager.NormalFamily, 16));
            
            Button disconnect = new Button(new Vector2(.535f, bandPosition.Y),
                                         new Vector2(0.15f,0.075f), "Disconnect", 0, DefaultFontColor, FontCache.Get(AssetManager.NormalFamily, 16));
            disconnect.Click += delegate{ Networking.NetworkManager.Disconnect(true); };
            
            connectToServer.Click += delegate{
                Player.MessageDispatcher.ShowNotification("Multiplayer is down.", Color.DarkRed, 3f, true);
            };
            
            Button options = new Button(new Vector2(.75f, bandPosition.Y),
                                        new Vector2(0.15f,0.075f), "Options", 0, DefaultFontColor, FontCache.Get(AssetManager.NormalFamily, 16));
            
            options.Click += delegate(object Sender, MouseButtonEventArgs E) { Menu.Disable(); OptionsMenu.Enable();};
            
            Button quit = new Button(new Vector2(.9f, bandPosition.Y),
                                     new Vector2(0.15f,0.075f), "Exit", 0, DefaultFontColor, FontCache.Get(AssetManager.NormalFamily, 16));
            
            quit.Click += delegate { Program.GameWindow.Exit(); };
            
            if( Program.GameWindow.GameVersion != "Unknown" ){
                GUIText versionText = new GUIText(Program.GameWindow.GameVersion, Vector2.Zero, Color.Black, FontCache.Get(AssetManager.NormalFamily, 8));
                versionText.Position = new Vector2(-1,1) + new Vector2(versionText.Scale.X, -versionText.Scale.Y);
                Menu.AddElement(versionText);
            }
            
            Menu.AddElement(blackBand);
            Menu.AddElement(connectToServer);
            Menu.AddElement(_title);
            Menu.AddElement(quit);
            Menu.AddElement(_newRun);
            Menu.AddElement(_loadButton);
            Menu.AddElement(options);
            Menu.AddElement(disconnect);
            //Menu.AddElement(Alpha);
            
            Menu.OnPanelStateChange += delegate(object Sender, PanelState E) { 
                if(E == PanelState.Enabled){
                    if(Networking.NetworkManager.IsConnected){
                        _newRun.Disable();
                        _loadButton.Disable();
                        connectToServer.Disable();
                        disconnect.Enable();
                    }else{
                        disconnect.Disable();
                    }
                }
            };
        }
        
        public void NewRunOnClick(object Sender, EventArgs E)
        {
            if(GameManager.InStartMenu){
                Menu.Disable();
                ChrChooser.Enable();
            }else{
                GameManager.NewRun(_player);
            }
        }
        
        public void Update()
        {
            if (_player == null) return;

            this.GamePanel.Update();
            _loadButton.Text.Text = GameManager.InStartMenu ? "Load World" : "Start Menu";
        }
        
        public void ShowMenu()
        {
            if(GameManager.IsLoading || GameManager.InMenu) return;

            Menu.Enable();
            OptionsMenu.Disable();
            ChrChooser.Disable();
            if(_player?.Inventory != null) _player.Inventory.Show = false;
            _player.AbilityTree.Show = false;
            GamePanel.Disable();
            ChrCreator.Disable();
            ConnectPanel.Disable();
            if(!Networking.NetworkManager.IsConnected)
            {
                GameSettings.Paused = true;
            }else{
                _player.View.LockMouse = false;
                _player.Movement.CaptureMovement = false;
                _player.View.CaptureMovement = false;
            }
            UpdateManager.CursorShown = true;
            LocalPlayer.Instance.Chat.Show = false;
            GameSettings.DarkEffect = false;
            System.Windows.Forms.Cursor.Position = new Point(GameSettings.Width / 2, GameSettings.Height/2);
        }
            
        public void HideMenu()
        {
            GameSettings.Paused = false;
            if(Networking.NetworkManager.IsConnected){
                _player.View.LockMouse = true;
                _player.Movement.CaptureMovement = true;
                _player.View.CaptureMovement = true;
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
            System.Windows.Forms.Cursor.Position = new Point(GameSettings.Width / 2, GameSettings.Height/2);
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
                var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                int k = 0 ;
                for(int i = 0; i < this.GetType().GetFields(flags).Length; i++)
                {
                    var field = this.GetType().GetFields(flags)[i];
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
