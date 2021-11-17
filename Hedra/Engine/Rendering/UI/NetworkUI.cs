/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/07/2016
 * Time: 12:11 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Collections.Generic;
using System.Numerics;
using Hedra.Game;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using SixLabors.ImageSharp;

namespace Hedra.Engine.Rendering.UI
{
    public class NetworkUI : Panel
    {
        private readonly List<UIElement> _hostButtons = new List<UIElement>();
        private readonly TextField _ipField;
        private readonly List<UIElement> _joinButtons = new List<UIElement>();

        public NetworkUI()
        {
            var fontSize = 14;
            var fontColor = Color.White;

            var bandPosition = new Vector2(0f, .8f);
            var blackBand = new BackgroundTexture(Color.FromRgb(69, 69, 69), Color.FromRgb(19, 19, 19),
                bandPosition, new Vector2(1f, 0.08f / GameSettings.Height * 578), GradientType.LeftRight);

            var hostTab = new Button(new Vector2(-0.1f, bandPosition.Y),
                new Vector2(0.15f, 0.075f), "Host", Color.White, FontCache.GetNormal(fontSize));

            hostTab.Click += delegate
            {
                SetHostButtonState(true);
                SetJoinButtonState(false);
            };

            var joinTab = new Button(new Vector2(0.1f, bandPosition.Y),
                new Vector2(0.15f, 0.075f), "Join", Color.White, FontCache.GetNormal(fontSize));
            joinTab.Click += delegate
            {
                SetHostButtonState(false);
                SetJoinButtonState(true);
            };

            var gameId = new GUIText("Game ID", new Vector2(0, -.25f), Color.White, FontCache.GetNormal(fontSize));
            _ipField = new TextField(new Vector2(0, -.4f), new Vector2(.20f, .05f));
            _ipField.Text = string.Empty;

            var join = new Button(new Vector2(0, -.65f), new Vector2(.15f, .05f), "Join", Color.White,
                FontCache.GetNormal(fontSize));
            join.Click += delegate { };

            var host = new Button(new Vector2(0, -.65f), new Vector2(.15f, .05f), "Host", Color.White,
                FontCache.GetNormal(fontSize));
            /*host.Click += delegate {
                if(NetworkManager.Host()){
                    
                    GameManager.InStartMenu = true;
                    Constants.REDIRECT_NET = false;
                    Game.NewRun(Game.CurrentInformation.Clone());
                    Game.LPlayer.UI.HideMenu();
                }else{
                    //Hacky stuff
                    if(NetworkManager.Host()){
                        GameManager.InStartMenu = true;
                        Constants.REDIRECT_NET = false;
                        Game.NewRun(Game.CurrentInformation.Clone());
                        Game.LPlayer.UI.HideMenu();
                    }else
                        LocalPlayer.Instance.MessageDispatcher.ShowNotification("Connection Refused",Color.FromArgb(255,229,10,10), 2.5f);
                }
            };*/

            _joinButtons.Add(_ipField);
            _joinButtons.Add(join);
            _joinButtons.Add(gameId);
            _hostButtons.Add(host);
            AddElement(blackBand);
            AddElement(joinTab);
            AddElement(hostTab);

            for (var i = 0; i < _joinButtons.Count; i++) AddElement(_joinButtons[i]);
            for (var i = 0; i < _hostButtons.Count; i++) AddElement(_hostButtons[i]);

            OnPanelStateChange += delegate(object Sender, PanelState E)
            {
                //if(e == PanelState.DISABLED){
                //    GraphicsOptions.DarkEffect = false;
                //}
                if (E == PanelState.Enabled)
                {
                    SetJoinButtonState(false);
                    SetHostButtonState(true);
                    GameSettings.DarkEffect = true;
                }
            };

            OnEscapePressed += delegate
            {
                Disable();
                GameManager.Player.UI.Menu.Enable();
            };
        }

        public void SetJoinButtonState(bool State)
        {
            for (var i = 0; i < _joinButtons.Count; i++)
                if (State)
                    _joinButtons[i].Enable();
                else
                    _joinButtons[i].Disable();
        }

        public void SetHostButtonState(bool State)
        {
            for (var i = 0; i < _hostButtons.Count; i++)
                if (State)
                    _hostButtons[i].Enable();
                else
                    _hostButtons[i].Disable();
        }
    }
}