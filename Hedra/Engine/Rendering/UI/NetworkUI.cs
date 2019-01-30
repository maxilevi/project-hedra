/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/07/2016
 * Time: 12:11 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using OpenTK;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Networking;
using System.Collections.Generic;
using Hedra.Engine.Game;
using Hedra.Engine.Player;

namespace Hedra.Engine.Rendering.UI
{
    public class NetworkUI : Panel
    {
        private TextField _ipField;
        private List<UIElement> _joinButtons = new List<UIElement>();
        private List<UIElement> _hostButtons = new List<UIElement>();
        
        public NetworkUI() : base(){
            int fontSize = 14;
            Color fontColor = Color.White;
            
            Vector2 bandPosition = new Vector2(0f, .8f);
            Texture blackBand = new Texture(Color.FromArgb(255,69,69,69), Color.FromArgb(255,19,19,19), bandPosition, new Vector2(1f, 0.08f / GameSettings.Height * 578), GradientType.LeftRight);
            
            Button hostTab = new Button(new Vector2(-0.1f, bandPosition.Y),
                                new Vector2(0.15f,0.075f), "Host", Color.White, FontCache.Get(AssetManager.NormalFamily, fontSize));
            
            hostTab.Click += delegate { 
                this.SetHostButtonState(true);
                this.SetJoinButtonState(false);
            };
            
            Button joinTab = new Button(new Vector2(0.1f, bandPosition.Y),
                                new Vector2(0.15f,0.075f), "Join", Color.White, FontCache.Get(AssetManager.NormalFamily, fontSize));
            joinTab.Click += delegate { 
                this.SetHostButtonState(false);
                this.SetJoinButtonState(true);
            };
            
            GUIText gameId = new GUIText("Game ID", new Vector2(0,-.25f), Color.White, FontCache.Get(AssetManager.NormalFamily, fontSize));
            _ipField = new TextField(new Vector2(0,-.4f), new Vector2(.20f,.05f), this);
            _ipField.Text = string.Empty;
            
            var join = new Button(new Vector2(0,-.65f), new Vector2(.15f,.05f), "Join", Color.White, FontCache.Get(AssetManager.NormalFamily, fontSize));
            join.Click += delegate {

            };
            
            Button host = new Button(new Vector2(0,-.65f), new Vector2(.15f,.05f), "Host", Color.White, FontCache.Get(AssetManager.NormalFamily, fontSize));
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

            this._joinButtons.Add(_ipField);
            this._joinButtons.Add(@join);
            this._joinButtons.Add(gameId);
            this._hostButtons.Add(host);
            this.AddElement(blackBand);
            this.AddElement(joinTab);
            this.AddElement(hostTab);
            
            for(int i = 0; i < _joinButtons.Count; i++){
                this.AddElement(_joinButtons[i]);
            }
            for(int i = 0; i < _hostButtons.Count; i++){
                this.AddElement(_hostButtons[i]);
            }
            
            OnPanelStateChange += delegate(object Sender, PanelState E) {
                //if(e == PanelState.DISABLED){
                //    GraphicsOptions.DarkEffect = false;
                //}
                if(E == PanelState.Enabled){
                    this.SetJoinButtonState(false);
                    this.SetHostButtonState(true);
                    GameSettings.DarkEffect = true;
                }
            };
            
            this.OnEscapePressed += delegate {
                this.Disable(); 
                GameManager.Player.UI.Menu.Enable();
            };
        }
        
        public void SetJoinButtonState(bool State){
            for(int i = 0; i < _joinButtons.Count; i++){
                if(State)
                    _joinButtons[i].Enable();
                else
                    _joinButtons[i].Disable();
            }
        }
        
        public void SetHostButtonState(bool State){
            for(int i = 0; i < _hostButtons.Count; i++){
                if(State)
                    _hostButtons[i].Enable();
                else
                    _hostButtons[i].Disable();
            }
        }
        
    }
}
