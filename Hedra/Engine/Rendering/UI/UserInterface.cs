/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 25/01/2016
 * Time: 09:49 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Networking;
using Hedra.Engine.Player;
using Hedra.Game;
using Hedra.Input;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Rendering.UI
{
    public class UserInterface
    {
        public static Vector2 BlackBandSize = new Vector2(1f, 0.054f);
        private static readonly Color DefaultFontColor = Color.White;
        private readonly HelpUI _helpPanel;
        private readonly Button _loadButton;
        private readonly OptionsUI _optionsMenu;
        private readonly IPlayer _player;
        private readonly BackgroundTexture _title;
        private readonly NetworkUI ConnectPanel;
        private bool _mEnabled;

        private bool _showHelp;

        private readonly List<bool> _wasEnabled = new List<bool>();

        public UserInterface(IPlayer Player)
        {
            _player = Player;

            Menu = new Panel();
            _optionsMenu = new OptionsUI();
            GamePanel = new GameUI(Player);
            CharacterSelector = new CharacterSelectorUI();
            ConnectPanel = new NetworkUI();
            CharacterCreator = new CharacterCreatorUI(Player);
            _helpPanel = new HelpUI();

            var bandPosition = new Vector2(0, -.8f);
            const int fontSize = 16;
            _title = new BackgroundTexture(Graphics2D.LoadFromAssets("Assets/UI/MenuLogo.png"),
                new Vector2(-.405f, .35f), Graphics2D.SizeFromAssets("Assets/UI/MenuLogo.png").As1920x1080() * .75f);

            var blackBand = new BackgroundTexture(Color.FromArgb(0, 69, 69, 69), Color.FromArgb(255, 19, 19, 19),
                bandPosition, BlackBandSize, GradientType.LeftRight);


            var newRun = new Button(new Vector2(.1f, bandPosition.Y),
                new Vector2(0.15f, 0.075f), Translation.Create("new_world"), DefaultFontColor,
                FontCache.GetNormal(fontSize));

            newRun.Click += delegate
            {
                GameSettings.NewWorld = true;
                CharacterSelector.ShouldHost = false;
                if (GameManager.InStartMenu)
                {
                    Menu.Disable();
                    CharacterSelector.Enable();
                }
                else
                {
                    GameManager.NewRun(_player);
                }
            };

            _loadButton = new Button(new Vector2(.3f, bandPosition.Y),
                new Vector2(0.15f, 0.075f), Translation.Create("load_world"), DefaultFontColor,
                FontCache.GetNormal(fontSize));
            _loadButton.Click += delegate
            {
                GameSettings.NewWorld = false;
                if (!GameManager.InStartMenu)
                {
                    AutosaveManager.Save();
                    GameManager.LoadMenu();
                }

                CharacterSelector.ShouldHost = false;
                Menu.Disable();
                CharacterSelector.Enable();
            };

            var inviteFriends = new Button(new Vector2(.3f, bandPosition.Y), Vector2.Zero,
                Translation.Create("invite_friends"), DefaultFontColor, FontCache.GetNormal(fontSize));

            var hostWorld = new Button(new Vector2(.535f, bandPosition.Y),
                new Vector2(0.15f, 0.075f), Translation.Create("host_world"), DefaultFontColor,
                FontCache.GetNormal(fontSize));

            var disconnect = new Button(new Vector2(.535f, bandPosition.Y),
                new Vector2(0.15f, 0.075f), Translation.Create("disconnect"), DefaultFontColor,
                FontCache.GetNormal(fontSize));

            inviteFriends.Click += delegate { Network.Instance.InviteFriends(); };

            hostWorld.Click += delegate
            {
                if (GameManager.InStartMenu)
                {
                    Menu.Disable();
                    CharacterSelector.Enable();
                    CharacterSelector.ShouldHost = true;
                }
                else
                {
                    Network.Instance.Host();
                    UpdateButtons();
                    disconnect.CanClick = false;
                    TaskScheduler.After(.05f, () => disconnect.CanClick = true);
                }
            };

            disconnect.Click += delegate
            {
                Network.Instance.Disconnect();
                UpdateButtons();
                hostWorld.CanClick = false;
                TaskScheduler.After(.05f, () => hostWorld.CanClick = true);
            };

            var options = new Button(new Vector2(.75f, bandPosition.Y),
                new Vector2(0.15f, 0.075f), Translation.Create("options"), DefaultFontColor,
                FontCache.GetNormal(fontSize));

            options.Click += delegate
            {
                Menu.Disable();
                _optionsMenu.Enable();
            };

            var quit = new Button(new Vector2(.9f, bandPosition.Y),
                new Vector2(0.15f, 0.075f), Translation.Create("exit"), DefaultFontColor,
                FontCache.GetNormal(fontSize));

            quit.Click += delegate { Program.GameWindow.Close(); };

            if (Program.GameWindow.GameVersion != "Unknown")
            {
                var versionText = new GUIText(Program.GameWindow.GameVersion, Vector2.Zero, Color.Black,
                    FontCache.GetNormal(8));
                versionText.Position = new Vector2(-1, 1) + new Vector2(versionText.Scale.X, -versionText.Scale.Y);
                Menu.AddElement(versionText);
            }

            Menu.AddElement(blackBand);
            Menu.AddElement(hostWorld);
            Menu.AddElement(_title);
            Menu.AddElement(quit);
            Menu.AddElement(newRun);
            Menu.AddElement(_loadButton);
            Menu.AddElement(options);
            Menu.AddElement(disconnect);
            Menu.AddElement(inviteFriends);

            Menu.OnPanelStateChange += delegate(object Sender, PanelState E)
            {
                if (E != PanelState.Enabled) return;
                hostWorld.Disable();
                //if(!Steam.Instance.IsAvailable) hostWorld.Disable();
                if (Network.Instance.IsAlive)
                {
                    newRun.Disable();
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

        public Panel Menu { get; }
        public GameUI GamePanel { get; }
        public CharacterSelectorUI CharacterSelector { get; }
        public CharacterCreatorUI CharacterCreator { get; }
        public bool InMenu => Menu.Enabled || _optionsMenu.Enabled || ConnectPanel.Enabled;

        public bool ShowHelp
        {
            get => _showHelp;
            set
            {
                if (value && Hide)
                    return;
                _showHelp = value;
                if (_showHelp)
                    _helpPanel.Enable();
                else
                    _helpPanel.Disable();
            }
        }

        public bool Hide
        {
            get => _mEnabled;
            set
            {
                var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                var k = 0;
                for (var i = 0; i < GetType().GetFields(flags).Length; i++)
                {
                    var field = GetType().GetFields(flags)[i];
                    if (typeof(Panel).IsAssignableFrom(field.FieldType))
                    {
                        if (value)
                        {
                            _wasEnabled.Add((field.GetValue(this) as Panel).Enabled);
                            (field.GetValue(this) as Panel).Disable();
                        }
                        else
                        {
                            if (!_mEnabled)
                                return;
                            if (_wasEnabled[k])
                                (field.GetValue(this) as Panel).Enable();
                        }

                        k++;
                    }
                }

                if (!value)
                    _wasEnabled.Clear();
                _mEnabled = value;
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
            if (GameSettings.ContinousMove) _player.View.LockMouse = false;
            if (GameManager.IsLoading || GameManager.InMenu || GameSettings.ContinousMove) return;

            if (ShowHelp)
                _helpPanel.Enable();
            else
                _helpPanel.Disable();
            Menu.Enable();
            _optionsMenu.Disable();
            CharacterSelector.ReloadSaveFile();
            CharacterSelector.Disable();
            _player.HideInterfaces();
            GamePanel.Disable();
            CharacterCreator.Disable();
            ConnectPanel.Disable();
            _helpPanel.Disable();
            if (!Network.Instance.IsAlive)
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
            CharacterSelector.Disable();
            CharacterCreator.Disable();
            ConnectPanel.Disable();
            Cursor.Show = false;
            Cursor.Center();
        }
    }
}