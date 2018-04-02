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
			                    new Vector2(0.15f,0.075f), "Host", 0, Color.White, FontCache.Get(UserInterface.Fonts.Families[0], fontSize));
			
			hostTab.Click += delegate { 
				this.SetHostButtonState(true);
				this.SetJoinButtonState(false);
			};
			
			Button joinTab = new Button(new Vector2(0.1f, bandPosition.Y),
			                    new Vector2(0.15f,0.075f), "Join", 0, Color.White, FontCache.Get(UserInterface.Fonts.Families[0], fontSize));
			joinTab.Click += delegate { 
				this.SetHostButtonState(false);
				this.SetJoinButtonState(true);
			};
			
			GUIText gameId = new GUIText("Game ID", new Vector2(0,-.25f), Color.White, FontCache.Get(UserInterface.Fonts.Families[0], fontSize));
			_ipField = new TextField(new Vector2(0,-.4f), new Vector2(.20f,.05f), this);
			_ipField.Text = "";
			
			var join = new Button(new Vector2(0,-.65f), new Vector2(.15f,.05f), "Join", 0, Color.White, FontCache.Get(UserInterface.Fonts.Families[0], fontSize));
			join.Click += delegate {
				
				if(!NetworkManager.Join(_ipField.Text)){
					LocalPlayer.Instance.MessageDispatcher.ShowNotification("Connection Refused",Color.FromArgb(255,229,10,10), 2.5f);
					return;
				}
				Constants.CHARACTER_CHOOSED = true;
				Constants.REDIRECT_NET = false;
				//Scenes.SceneManager.Game.MakeCurrent(Scenes.SceneManager.Game.CurrentInformation);
				if(NetworkManager.WorldSeed != -1)
					World.Recreate(NetworkManager.WorldSeed);
				if(NetworkManager.WorldTime != -1)
					Enviroment.SkyManager.SetTime(NetworkManager.WorldTime);
				Scenes.SceneManager.Game.Player.Spawner.Enabled = false;
				Scenes.SceneManager.Game.Player.UI.HideMenu();
				Scenes.SceneManager.Game.Player.BlockPosition = GameSettings.SpawnPoint.ToVector3() + Vector3.UnitY * 128;
				
			};
			
			Button host = new Button(new Vector2(0,-.65f), new Vector2(.15f,.05f), "Host", 0, Color.White, FontCache.Get(UserInterface.Fonts.Families[0], fontSize));
			/*host.Click += delegate {
				if(NetworkManager.Host()){
					
					Constants.CHARACTER_CHOOSED = true;
					Constants.REDIRECT_NET = false;
					Scenes.SceneManager.Game.NewRun(Scenes.SceneManager.Game.CurrentInformation.Clone());
					Scenes.SceneManager.Game.LPlayer.UI.HideMenu();
				}else{
					//Hacky stuff
					if(NetworkManager.Host()){
						Constants.CHARACTER_CHOOSED = true;
						Constants.REDIRECT_NET = false;
						Scenes.SceneManager.Game.NewRun(Scenes.SceneManager.Game.CurrentInformation.Clone());
						Scenes.SceneManager.Game.LPlayer.UI.HideMenu();
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
				//	GraphicsOptions.DarkEffect = false;
				//}
				if(E == PanelState.Enabled){
					this.SetJoinButtonState(false);
					this.SetHostButtonState(true);
					GameSettings.DarkEffect = true;
				}
			};
			
			this.OnEscapePressed += delegate {
				this.Disable(); 
				Scenes.SceneManager.Game.Player.UI.Menu.Enable();
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
