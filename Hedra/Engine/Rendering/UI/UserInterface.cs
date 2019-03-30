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
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Input;
using Hedra.Engine.Localization;
using OpenTK;
using Hedra.Engine.Management;
using Hedra.Engine.Networking;
using Hedra.Engine.Player;

namespace Hedra.Engine.Rendering.UI
{
    public class UserInterface
    {
        private readonly IPlayer _player;
        public Panel Menu { get; }
        private readonly OptionsUI _optionsMenu;
        public GameUI GamePanel { get; }
        public ChrChooserUI ChrChooser { get; }
        public ChrCreatorUI ChrCreator { get; }
        private readonly NetworkUI ConnectPanel;
        private readonly Texture _title;
        private readonly Button _newRun;
        private readonly Button _loadButton;
        public bool InMenu => Menu.Enabled || _optionsMenu.Enabled || ConnectPanel.Enabled;
        private static readonly Color DefaultFontColor = Color.White;
        
        public UserInterface (IPlayer Player)
        {
            this._player = Player;
            
            Menu = new Panel();
            _optionsMenu = new OptionsUI();
            GamePanel = new GameUI(Player);
            ChrChooser = new ChrChooserUI(Player);
            ConnectPanel = new NetworkUI();
            ChrCreator = new ChrCreatorUI(Player);

            var bandPosition = new Vector2(0, -.8f);
            const int fontSize = 16;
            _title = new Texture(Graphics2D.LoadFromAssets("Assets/UI/MenuLogo.png"),
                                   new Vector2(-.405f, .35f), Graphics2D.SizeFromAssets("Assets/UI/MenuLogo.png").As1920x1080() * .75f);

            var blackBand = new Texture(Color.FromArgb(0,69,69,69), Color.FromArgb(255,19,19,19), bandPosition, new Vector2(1f, 0.09f / GameSettings.Height * 578), GradientType.LeftRight);
            
            
            _newRun = new Button(new Vector2(.1f, bandPosition.Y),
                                new Vector2(0.15f,0.075f), Translation.Create("new_world"), DefaultFontColor, FontCache.GetNormal(fontSize));

            _newRun.Click += delegate
            {
                ChrChooser.ShouldHost = false;
                if(GameManager.InStartMenu)
                {
                    Menu.Disable();
                    ChrChooser.Enable();
                }
                else
                {
                    GameManager.NewRun(_player);
                }
            };
            
            _loadButton = new Button(new Vector2(.3f, bandPosition.Y),
                                         new Vector2(0.15f,0.075f), Translation.Create("load_world"), DefaultFontColor, FontCache.GetNormal(fontSize));
            _loadButton.Click += delegate
            {
                if(!GameManager.InStartMenu)
                {
                    AutosaveManager.Save();
                    GameManager.LoadMenu();
                }
                ChrChooser.ShouldHost = false;
                Menu.Disable();
                ChrChooser.Enable();
            };
            
            var inviteFriends = new Button(new Vector2(.3f, bandPosition.Y), Vector2.Zero,
                Translation.Create("invite_friends"), DefaultFontColor, FontCache.GetNormal(fontSize));
            
            var hostWorld = new Button(new Vector2(.535f, bandPosition.Y),
                                         new Vector2(0.15f,0.075f), Translation.Create("host_world"), DefaultFontColor, FontCache.GetNormal(fontSize));
            
            var disconnect = new Button(new Vector2(.535f, bandPosition.Y),
                                         new Vector2(0.15f,0.075f), Translation.Create("disconnect"), DefaultFontColor, FontCache.GetNormal(fontSize));
            
            inviteFriends.Click += delegate
            {
                Connection.Instance.InviteFriends();
            };
            
            hostWorld.Click += delegate
            {
                if (GameManager.InStartMenu)
                {
                    Menu.Disable();
                    ChrChooser.Enable();
                    ChrChooser.ShouldHost = true;
                }
                else
                {
                    Connection.Instance.Host();
                    UpdateButtons();
                    disconnect.CanClick = false;
                    TaskScheduler.After(.05f, () => disconnect.CanClick = true);
                }
            };
            
            disconnect.Click += delegate
            {
                Connection.Instance.Disconnect();
                UpdateButtons();
                hostWorld.CanClick = false;
                TaskScheduler.After(.05f, () => hostWorld.CanClick = true);
            };
            
            Button options = new Button(new Vector2(.75f, bandPosition.Y),
                                        new Vector2(0.15f,0.075f), Translation.Create("options"), DefaultFontColor, FontCache.GetNormal(fontSize));
            
            options.Click += delegate
            {
                Menu.Disable(); _optionsMenu.Enable();
            };
            
            Button quit = new Button(new Vector2(.9f, bandPosition.Y),
                                     new Vector2(0.15f,0.075f), Translation.Create("exit"), DefaultFontColor, FontCache.GetNormal(fontSize));
            
            quit.Click += delegate { Program.GameWindow.Exit(); };
            
            if(Program.GameWindow.GameVersion != "Unknown")
            {
                var versionText = new GUIText(Program.GameWindow.GameVersion, Vector2.Zero, Color.Black, FontCache.GetNormal(8));
                versionText.Position = new Vector2(-1,1) + new Vector2(versionText.Scale.X, -versionText.Scale.Y);
                Menu.AddElement(versionText);
            }
            
            Menu.AddElement(blackBand);
            Menu.AddElement(hostWorld);
            Menu.AddElement(_title);
            Menu.AddElement(quit);
            Menu.AddElement(_newRun);
            Menu.AddElement(_loadButton);
            Menu.AddElement(options);
            Menu.AddElement(disconnect);
            Menu.AddElement(inviteFriends);
            
            Menu.OnPanelStateChange += delegate(object Sender, PanelState E)
            {
                if (E != PanelState.Enabled) return;
                if (Connection.Instance.IsAlive)
                {

                    _newRun.Disable();
                    _loadButton.Disable();
                    hostWorld.Disable();
                    disconnect.Enable();
                    inviteFriends.Enable();
                }
                else
                {
                    inviteFriends.Disable();
                    disconnect.Disable();
                }
            };

            void UpdateButtons()
            {
                Menu.Disable();
                Menu.Enable();
            }
        }
        
        public void Update()
        {
            if (_player == null) return;

            _optionsMenu.Update();
            GamePanel.Update();
            _loadButton.Text.Text = GameManager.InStartMenu 
                ? Translations.Get("load_world") 
                : Translations.Get("start_menu");
        }
        
        public void ShowMenu()
        {
            if (GameSettings.ContinousMove)
            {
                _player.View.LockMouse = false;
            }
            if(GameManager.IsLoading || GameManager.InMenu || GameSettings.ContinousMove) return;

            Menu.Enable();
            _optionsMenu.Disable();
            ChrChooser.Disable();
            _player.HideInterfaces();
            GamePanel.Disable();
            ChrCreator.Disable();
            ConnectPanel.Disable();
            if(!Connection.Instance.IsAlive)
            {
                GameSettings.Paused = true;
            }
            else
            {
                _player.View.LockMouse = false;
                _player.View.CaptureMovement = false;
                _player.CanInteract = false;
            }
            Cursor.Show = true;
            LocalPlayer.Instance.Chat.Show = false;
            GameSettings.DarkEffect = false;
            Cursor.Center();
        }

        public void HideMenu()
        {
            GameSettings.Paused = false;
            _player.View.LockMouse = true;
            _player.View.CaptureMovement = true;
            _player.CanInteract = true;
            Menu.Disable();
            _optionsMenu.Disable();
            GamePanel.Enable();
            ChrChooser.Disable();
            ChrCreator.Disable();
            ConnectPanel.Disable();
            Cursor.Show = false;
            LocalPlayer.Instance.Chat.Show = true;
            LocalPlayer.Instance.Chat.LoseFocus();
            Cursor.Center();
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
