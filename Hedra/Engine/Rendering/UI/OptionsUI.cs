/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/07/2016
 * Time: 06:14 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
 #define DONATE_BTC

using Hedra.Engine.Management;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Linq;

namespace Hedra.Engine.Rendering.UI
{
    /// <summary>
    /// Description of OptionsUI.
    /// </summary>
    public class OptionsUI : Panel
	{
	    public Button DonateBtcButton = null;
        private List<UIElement> _graphicsButtons = new List<UIElement>();
		private List<UIElement> _audioButtons = new List<UIElement>();
		private List<UIElement> _inputButtons = new List<UIElement>();
		private List<UIElement> _displayButtons = new List<UIElement>();
	    private readonly Button _audio;
	    private readonly Button _graphics;
	    private readonly Button _display;
	    private readonly Button _input;
	    private readonly Button _controls;
	    private ControlsUi _controlsText;
		private Vector2 _previousOffset;
	    private readonly Font _normalFont;
	    private readonly Font _boldFont;

		public OptionsUI() : base()
		{
			var fontSize = 14;
			var dist = .2f;
			var vDist = .25f;
            _normalFont = FontCache.Get(UserInterface.Fonts.Families[0], fontSize-1);
		    _boldFont = FontCache.Get(AssetManager.Fonts.Families[0], fontSize+1, FontStyle.Bold);
            Color fontColor = Color.White;
			_controlsText = new ControlsUi(fontColor);
			
			var bandPosition = new Vector2(0f, .8f);
			var blackBand = new Texture(Color.FromArgb(255,69,69,69), Color.FromArgb(255,19,19,19), bandPosition, new Vector2(1f, 0.08f / GameSettings.Height * 578), GradientType.LEFT_RIGHT);

		    var donateBtc = new Texture(Graphics2D.LoadFromAssets("Assets/UI/SocialMedia.png"), Vector2.Zero, Vector2.One);
			donateBtc.Disable();
			
			Button copyAddress = new Button(new Vector2(0.275f, 0.475f), Graphics2D.SizeFromAssets("Assets/UI/CopyIcon.png") * .05f, Graphics2D.LoadFromAssets("Assets/UI/CopyIcon.png"));
			copyAddress.Disable();

		    Vector2 squareScale = new Vector2(0.3f, 0.55f) * .8f;
            Button discordClick = new Button(new Vector2(0f, -.05f), squareScale, 0);
		    var redditClick = new Button(new Vector2(-.6f, -.05f), squareScale, 0);
		    var twitterClick = new Button(new Vector2(.6f, -.05f), squareScale, 0);

            DonateBtcButton = new Button(Vector2.Zero, Vector2.One, 0);

            /*copyAddress.Click += delegate {
				DonateBtcButton.Clickable = false;
				Clipboard.SetText("18GbXbnGzLDwi5KmZ1VD4JQpibw6Lpyfvp");
				Player.MessageDispatcher.ShowNotification("Succesfully copied to clipboard", Color.White, 3f, true);
				TaskManager.RunAfterSeconds(250, delegate{ DonateBtcButton.Clickable = true; } );
			};*/


		    DonateBtcButton.Click += delegate {
		        donateBtc.Disable();
		        DonateBtcButton.Disable();
		        twitterClick.Disable();
		        discordClick.Disable();
		        redditClick.Disable();
		        //copyAddress.Disable();
		        Enable();
		    };

            twitterClick.Click += delegate
		    {
		        DonateBtcButton.Clickable = false;
                Process.Start("https://twitter.com/Zaphyk");
		        TaskManager.Delay(250, delegate { DonateBtcButton.Clickable = true; });
            };

		    redditClick.Click += delegate
		    {
		        DonateBtcButton.Clickable = false;
                Process.Start("https://www.reddit.com/r/projecthedra");
		        TaskManager.Delay(250, delegate { DonateBtcButton.Clickable = true; });
            };

		    discordClick.Click += delegate
		    {
		        DonateBtcButton.Clickable = false;
                Process.Start("https://discord.gg/AEC4Uab");
		        TaskManager.Delay(250, delegate { DonateBtcButton.Clickable = true; });
            };

			
			Button supportGame = new Button(new Vector2(0f, -bandPosition.Y),  Graphics2D.SizeFromAssets("Assets/UI/SocialButton.png") * .5f, Graphics2D.LoadFromAssets("Assets/UI/SocialButton.png"));
			supportGame.Click += delegate { 
				Disable();
				donateBtc.Enable();
				DonateBtcButton.Enable();
			    twitterClick.Enable();
			    discordClick.Enable();
			    redditClick.Enable();
                //copyAddress.Enable();
            };
			
			_graphics = new Button(new Vector2(0f, bandPosition.Y),
			                    new Vector2(0.15f,0.075f), "Graphics", 0, Color.White, _normalFont);
			_graphics.Click += delegate
			{

			    this.SetGraphicsButtonState(true);
			    this.SetAudioButtonState(false);
			    this.SetInputButtonState(false);
			    this.SetDisplayButtonState(false);
			    this.SetControlsButtonState(false);
			};
			
			_audio = new Button(new Vector2(-0.2f, bandPosition.Y),
			                    new Vector2(0.15f,0.075f), "Audio", 0, Color.White, _normalFont);
			
			_audio.Click += delegate { 
				SetGraphicsButtonState(false);
				SetAudioButtonState(true);
				SetInputButtonState(false);
				SetDisplayButtonState(false);
				SetControlsButtonState(false);
			};
			
			_input = new Button(new Vector2(-0.4f, bandPosition.Y),
			                    new Vector2(0.15f,0.075f), "Input", 0, Color.White, _normalFont);
			
			_input.Click += delegate { 
				SetGraphicsButtonState(false);
				SetAudioButtonState(false);
				SetInputButtonState(true);
				SetDisplayButtonState(false);
				SetControlsButtonState(false);
			};
			
			_display = new Button(new Vector2(0.2f,bandPosition.Y), new Vector2(0.15f,0.05f),
			                           "Display", 0, Color.White, _normalFont);
			
			_display.Click += delegate { 
				SetGraphicsButtonState(false);
				SetAudioButtonState(false);
				SetInputButtonState(false);
				SetDisplayButtonState(true);
				SetControlsButtonState(false);
			};
			
			_controls = new Button(new Vector2(0.4f,bandPosition.Y), new Vector2(0.15f,0.05f),
			                           "Controls", 0, Color.White, _normalFont);
			_controls.Click += delegate { 
				SetGraphicsButtonState(false);
				SetAudioButtonState(false);
				SetInputButtonState(false);
				SetDisplayButtonState(false);
				SetControlsButtonState(true);
			};
			
			
			GameSettings.ChunkLoaderRadius = Math.Max(GameSettings.ChunkLoaderRadius, GameSettings.MinLoadingRadius);
		    var viewValuesList = new List<string>();
		    for (int i = GameSettings.MinLoadingRadius; i < GameSettings.MaxLoadingRadius + 1; i++)
		    {
		        viewValuesList.Add(i.ToString());
		    }


            var viewValues = viewValuesList.ToArray();
			
			OptionChooser viewDistance = new OptionChooser(new Vector2(dist, vDist*2f), new Vector2(0.15f, 0.075f), "View Distance: ",
			                                 fontColor, _normalFont,
			                                viewValues, false);
			viewDistance.Index = (GameSettings.MaxLoadingRadius - GameSettings.MinLoadingRadius) / 2;
			viewDistance.CurrentValue.Text = viewValues[(GameSettings.MaxLoadingRadius - GameSettings.MinLoadingRadius) / 2];
			
			for(int i = 0; i < viewValues.Length; i++){
				if(int.Parse(viewValues[i]) == GameSettings.ChunkLoaderRadius){
					viewDistance.CurrentValue.Text	= viewValues[i];
					viewDistance.Index = i;
				}
			}
			
			viewDistance.LeftArrow.Click += delegate { 
				GameSettings.ChunkLoaderRadius = viewDistance.Index + GameSettings.MinLoadingRadius;
			};
			
			viewDistance.RightArrow.Click += delegate { 
				GameSettings.ChunkLoaderRadius = viewDistance.Index + GameSettings.MinLoadingRadius;
			};

		    var fpsLimitList = new List<string>();
            fpsLimitList.AddRange(Enumerable.Range(0, 12+1).Select(I => (I*5+30).ToString()));
            fpsLimitList.Add("NONE");
		    var frameLimiterValues = fpsLimitList.ToArray();


            var frameLimiter = new OptionChooser(new Vector2(-dist, 0), new Vector2(0.15f, 0.075f), "FPS Limit: ",
		        fontColor, _normalFont,
                frameLimiterValues);

		    if (GameSettings.FpsLimit <= 0.0f)
		        frameLimiter.Index = fpsLimitList.Count - 1;
		    else
		        frameLimiter.Index = Enumerable.Range(0, fpsLimitList.Count-1)
                    .FirstOrDefault(I => GameSettings.FpsLimit == int.Parse(fpsLimitList[I]));
		    frameLimiter.CurrentValue.Text = fpsLimitList[frameLimiter.Index];

            OnButtonClickEventHandler updateLimiter = delegate
            {
                if (frameLimiter.CurrentValue.Text == "NONE")
                    GameSettings.FpsLimit = 0.0f;
                else
                    GameSettings.FpsLimit = int.Parse(frameLimiter.CurrentValue.Text);
            };

		    frameLimiter.LeftArrow.Click += updateLimiter;
		    frameLimiter.RightArrow.Click += updateLimiter;

            var fxaa = new Button(new Vector2(dist, 0f),
                     new Vector2(0.15f, 0.075f), "FXAA: " + (GameSettings.FXAA ? "ON" : "OFF"), 0, fontColor, _normalFont);

            fxaa.Click += delegate {
                    GameSettings.FXAA = !GameSettings.FXAA;
                    fxaa.Text.Text = "FXAA: " + (GameSettings.FXAA ? "ON" : "OFF");
                };

            Button bloom = new Button(new Vector2(dist, -vDist),
                     new Vector2(0.15f, 0.075f), "Bloom: " + (GameSettings.Bloom ? "ON" : "OFF"), 0, fontColor, _normalFont);


            bloom.Click += delegate {
                    GameSettings.Bloom = !GameSettings.Bloom;
                    bloom.Text.Text = "Bloom: " + (GameSettings.Bloom ? "ON" : "OFF");
                };
            bloom.Disable();

            Button quality = new Button(new Vector2(-dist, vDist*2),
			                     new Vector2(0.15f,0.075f), "Quality: " + ( GameSettings.Fancy ? "FANCY" : "FAST"), 0, fontColor, _normalFont);
			
			quality.Click += new OnButtonClickEventHandler(
				delegate{
					if(GameSettings.Fancy)
						GameSettings.Fancy = false;
					else
						GameSettings.Fancy = true;
					quality.Text.Text = "Quality: " + ( GameSettings.Fancy ? "FANCY" : "FAST");
				});

			
			Button vSync = new Button(new Vector2(-dist, vDist),
			                     new Vector2(0.15f,0.075f), "VSync: " + ( GameSettings.VSync ? "ON" : "OFF"), 0, fontColor, _normalFont);
			
			vSync.Click += new OnButtonClickEventHandler(
				delegate{
					if(GameSettings.VSync)
						GameSettings.VSync = false;
					else
						GameSettings.VSync = true;
					vSync.Text.Text = "VSync: " + ( GameSettings.VSync ? "ON" : "OFF");
				});
			
			Button invertMouse = new Button(new Vector2(0, .6f),
			                     new Vector2(0.15f,0.075f), "Invert Mouse: " + ( GameSettings.InvertMouse ? "ON" : "OFF"), 0, fontColor, _normalFont);
			
			invertMouse.Click += new OnButtonClickEventHandler(
				delegate{
					if(GameSettings.InvertMouse)
						GameSettings.InvertMouse = false;
					else
						GameSettings.InvertMouse = true;
					invertMouse.Text.Text = "Invert Mouse: " + ( GameSettings.InvertMouse ? "ON" : "OFF");
				});
			
			string[] shadowsValues =  new string[]{"MEDIUM","LOW","MEDIUM","HIGH"};//Repeat medium so the option chooser has the correct size
			OptionChooser shadows = new OptionChooser(new Vector2(dist, vDist), new Vector2(0.15f, 0.075f), "Shadow Quality: ",
			                                 fontColor, _normalFont,
			                                shadowsValues, false);
			shadows.Index = 3;
			shadows.CurrentValue.Text = "MEDIUM";
			
			for(int i = 0; i < shadowsValues.Length; i++){
				if( i == GameSettings.ShadowQuality){
					shadows.CurrentValue.Text = (i == 0) ? "OFF" : shadowsValues[i];
					shadows.Index = i;
					break;
				}
			}
			
			shadows.LeftArrow.Click += delegate {
				GameSettings.ShadowQuality = shadows.Index;
				if(shadows.Index == 0)
					shadows.CurrentValue.Text = "OFF";
			};
			
			shadows.RightArrow.Click += delegate {
				GameSettings.ShadowQuality = shadows.Index;
				if(shadows.Index == 0)
					shadows.CurrentValue.Text = "OFF";
			};
			
			Button ssao = new Button(new Vector2(-dist, -vDist),
			                     new Vector2(0.15f,0.075f), "Ambient Occlusion: " + ( GameSettings.SSAO ? "ON" : "OFF"), 0, fontColor, _normalFont);
			
			ssao.Click += new OnButtonClickEventHandler(
				delegate{
					if(GameSettings.SSAO)
						GameSettings.SSAO = false;
					else
						GameSettings.SSAO = true;
					ssao.Text.Text = "Ambient Occlusion: " + ( GameSettings.SSAO ? "ON" : "OFF");
				});
			
			Button showChat = new Button(new Vector2(0, .2f),
			                     new Vector2(0.15f,0.075f), "Show Chat: " + ( GameSettings.ShowChat ? "ON" : "OFF"), 0, fontColor, _normalFont);
			
			showChat.Click += new OnButtonClickEventHandler(
				delegate{
					if(GameSettings.ShowChat)
						GameSettings.ShowChat = false;
					else
						GameSettings.ShowChat = true;
					showChat.Text.Text = "Show Chat: " + ( GameSettings.ShowChat ? "ON" : "OFF");
				});
			
			Button showMinimap = new Button(new Vector2(0, .4f),
			                     new Vector2(0.15f,0.075f), "Show Minimap: " + ( GameSettings.ShowMinimap ? "ON" : "OFF"), 0, fontColor, _normalFont);
			
			showMinimap.Click += new OnButtonClickEventHandler(
				delegate{
					if(GameSettings.ShowMinimap)
						GameSettings.ShowMinimap = false;
					else
						GameSettings.ShowMinimap = true;
					showMinimap.Text.Text = "Show Minimap: " + ( GameSettings.ShowMinimap ? "ON" : "OFF");
				});
			
			/*Button Fullscreen = new Button(new Vector2(dist, -vDist),
			                     new Vector2(0.15f,0.075f), "Fullscreen: " + ( GraphicsOptions.Fullscreen ? "ON" : "OFF"), 0, fontColor, new Font(UserInterface.Fonts.Families[0], fontSize));
						
			Fullscreen.Click += delegate{
					GraphicsOptions.Fullscreen = !GraphicsOptions.Fullscreen;
					Fullscreen.Text.Text = "Fullscreen: " + ( GraphicsOptions.Fullscreen ? "ON" : "OFF");
				};*/
			/*
			IList<DisplayResolution> AvailableResolutions = DisplayDevice.GetDisplay(DisplayIndex.Primary).AvailableResolutions;
			List<string> Resolutions = new List<string>();
			for(int i = 0; i < AvailableResolutions.Count; i++){
				if(AvailableResolutions[i].Width < 1024 || AvailableResolutions[i].Height < 578) continue;
				if(Resolutions.Contains(AvailableResolutions[i].Width + "x" + AvailableResolutions[i].Height))continue;
				Resolutions.Add( AvailableResolutions[i].Width + "x" + AvailableResolutions[i].Height);
			}
			
			OptionChooser Resolution = new OptionChooser(new Vector2(Dist, -VDist*3), new Vector2(0.15f, 0.075f), "Resolution: ",  FontColor, FontCache.Get(UserInterface.Fonts.Families[0], FontSize),
			                                             Resolutions.ToArray(), false);
			
			
			Resolution.Index = Resolutions.Count-1;
			Resolution.CurrentValue.Text = Resolutions[Resolutions.Count-1];
			
			Resolution.LeftArrow.Click += delegate { 
				GraphicsOptions.Resolution = Resolutions[Resolution.Index];
			};
			
			Resolution.RightArrow.Click += delegate { 
				GraphicsOptions.Resolution = Resolutions[Resolution.Index];
			};
			*/
			string[] volumeOptions = new string[]{"0%","5%","10%","15%","20%","25%","30%","35%","40%","45%","50%","55%","60%","65%","70%","75%","80%","85%","90%","95%","100%"};
			OptionChooser musicVolume = new OptionChooser(new Vector2(0, .6f), new Vector2(0.15f, 0.075f), "Music Volume: ",  fontColor, _normalFont,
			                                             volumeOptions, false);

			for(int i = 0; i < volumeOptions.Length; i++){
				if( (float) (Int32.Parse( volumeOptions[i].Replace("%","") ) / 100f) == SoundtrackManager.Volume){
					musicVolume.Index = i;
					musicVolume.CurrentValue.Text = volumeOptions[i];
					//SoundtrackManager.Volume = Int32.Parse( VolumeOptions[Volume.Index].Replace("%","") ) / 100f;				
					break;
				}
			}
			
			musicVolume.LeftArrow.Click += delegate { 
				SoundtrackManager.Volume = Int32.Parse( volumeOptions[musicVolume.Index].Replace("%","") ) / 100f;
			};
			
			musicVolume.RightArrow.Click += delegate { 
				SoundtrackManager.Volume = Int32.Parse( volumeOptions[musicVolume.Index].Replace("%","") ) / 100f;
			};
			
			OptionChooser sfxVolume = new OptionChooser(new Vector2(0, .4f), new Vector2(0.15f, 0.075f), "Sound FX Volume: ",  fontColor, _normalFont,
			                                             volumeOptions, false);

			for(int i = 0; i < volumeOptions.Length; i++){
				if( (float) (Int32.Parse( volumeOptions[i].Replace("%","") ) / 100f) == Sound.SoundManager.Volume){
					sfxVolume.Index = i;
					sfxVolume.CurrentValue.Text = volumeOptions[i];			
					break;
				}
			}
			
			sfxVolume.LeftArrow.Click += delegate { 
				Sound.SoundManager.Volume = Int32.Parse( volumeOptions[sfxVolume.Index].Replace("%","") ) / 100f;
			};
			
			sfxVolume.RightArrow.Click += delegate { 
				Sound.SoundManager.Volume = Int32.Parse( volumeOptions[sfxVolume.Index].Replace("%","") ) / 100f;
			};
			
			//if(!GraphicsOptions.MaxResolution)
			//	FullScreen.Clickable = false;
			//Resolution.Clickable = false;
			
			string[] sensitivityOptions = new string[]{"0.1","0.2","0.3","0.4","0.5","0.6","0.7","0.8","0.9","1.0","1.1","1.2","1.3","1.4","1.5","1.6","1.7","1.8","1.9","2.0","2.1","2.2","2.3","2.4","2.5"};
			OptionChooser mouseSensitivity = new OptionChooser(new Vector2(0, .4f), Vector2.Zero, "Mouse Sensitivity: ",
			                                                   fontColor, _normalFont, sensitivityOptions, false);
			mouseSensitivity.LeftArrow.Click += delegate {
				GameSettings.MouseSensibility = float.Parse(sensitivityOptions[mouseSensitivity.Index], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"));
			};
			
			mouseSensitivity.RightArrow.Click += delegate { 
				GameSettings.MouseSensibility = float.Parse(sensitivityOptions[mouseSensitivity.Index], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"));
			};
			
			for(int i = 0; i < sensitivityOptions.Length; i++){
				if(float.Parse(sensitivityOptions[i], NumberStyles.Any, CultureInfo.GetCultureInfo("en-US")) == GameSettings.MouseSensibility){
					mouseSensitivity.CurrentValue.Text	= sensitivityOptions[i];
					mouseSensitivity.Index = i;
				}
			}
			
			Button autosave = new Button(new Vector2(0f, .2f),
			                     new Vector2(0.15f,0.075f), "Autosave: " + ( GameSettings.Autosave ? "ON" : "OFF"), 0, fontColor, _normalFont);
			
			autosave.Click += new OnButtonClickEventHandler(
				delegate{
					if(GameSettings.Autosave)
						GameSettings.Autosave = false;
					else
						GameSettings.Autosave = true;
					autosave.Text.Text = "Autosave: " + ( GameSettings.Autosave ? "ON" : "OFF");
				});

            _graphicsButtons.Add(quality);
			_graphicsButtons.Add(vSync);
			_graphicsButtons.Add(shadows);
			_graphicsButtons.Add(ssao);
			_graphicsButtons.Add(viewDistance);
            _graphicsButtons.Add(fxaa);
		    _graphicsButtons.Add(frameLimiter);
            //_graphicsButtons.Add(Fullscreen);
            _inputButtons.Add(invertMouse);
			_inputButtons.Add(mouseSensitivity);
			_inputButtons.Add(autosave);
			_audioButtons.Add(musicVolume);
			_audioButtons.Add(sfxVolume);
			_displayButtons.Add(showChat);
			_displayButtons.Add(showMinimap);
			
			AddElement(_controls);
			AddElement(blackBand);
			AddElement(_audio);
			AddElement(_input);
			AddElement(_graphics);
			AddElement(_display);
			AddElement(supportGame);
			
			
			for(int i = 0; i < _graphicsButtons.Count; i++){
				AddElement(_graphicsButtons[i]);
			}
			for(int i = 0; i < _inputButtons.Count; i++){
				AddElement(_inputButtons[i]);
			}
			for(int i = 0; i < _audioButtons.Count; i++){
				AddElement(_audioButtons[i]);
			}
			for(int i = 0; i < _displayButtons.Count; i++){
				AddElement(_displayButtons[i]);
			}
			for(int i = 0; i < _controlsText.ControlsElements.Count; i++){
				AddElement(_controlsText.ControlsElements[i]);
			}
			
			Disable();
			
			OnEscapePressed += delegate { 
				Disable();
				Scenes.SceneManager.Game.LPlayer.UI.Menu.Enable();
			};
			
			OnPanelStateChange += delegate(object Sender, PanelState E) {
				if(E == PanelState.Disabled){
					GameSettings.Save(AssetManager.AppData+"settings.cfg");
					GameSettings.DarkEffect = false;
				}
				if(E == PanelState.Enabled){
					SetGraphicsButtonState(true);
					SetAudioButtonState(false);
					SetInputButtonState(false);
					SetDisplayButtonState(false);
					SetControlsButtonState(false);
					GameSettings.DarkEffect = true;
                    
					
				}
			};
		}

	    private void ResetFonts()
	    {
	        _graphics.Text.TextFont = _normalFont;
	        _input.Text.TextFont = _normalFont;
	        _audio.Text.TextFont = _normalFont;
	        _display.Text.TextFont = _normalFont;
	        _controls.Text.TextFont = _normalFont;
        }

	    private void UpdateFonts()
	    {
	        _graphics.Text.Update();
	        _input.Text.Update();
            _audio.Text.Update();
            _display.Text.Update();
            _controls.Text.Update();
        }

	    private void SetGraphicsButtonState(bool Enabled)
		{
		    if (Enabled)
		    {
		        this.ResetFonts();
		        _graphics.Text.TextFont = _boldFont;
		        this.UpdateFonts();
		    }
		    for(int i = 0; i < _graphicsButtons.Count; i++){
				if(Enabled)
					_graphicsButtons[i].Enable();
				else
					_graphicsButtons[i].Disable();
			}
		}

	    private void SetInputButtonState(bool Enabled){
	        if (Enabled)
	        {
	            this.ResetFonts();
	            _input.Text.TextFont = _boldFont;
	            this.UpdateFonts();
	        }
	        for (int i = 0; i < _inputButtons.Count; i++){
				if(Enabled)
					_inputButtons[i].Enable();
				else
					_inputButtons[i].Disable();
			}
			
		}

	    private void SetAudioButtonState(bool Enabled){
	        if (Enabled)
	        {
	            this.ResetFonts();
	            _audio.Text.TextFont = _boldFont;
	            this.UpdateFonts();
	        }
	        for (int i = 0; i < _audioButtons.Count; i++){
				if(Enabled)
					_audioButtons[i].Enable();
				else
					_audioButtons[i].Disable();
			}
		}

	    private void SetDisplayButtonState(bool Enabled){
	        if (Enabled)
	        {
	            this.ResetFonts();
	            _display.Text.TextFont = _boldFont;
	            this.UpdateFonts();
	        }
	        for (int i = 0; i < _displayButtons.Count; i++){
				if(Enabled)
					_displayButtons[i].Enable();
				else
					_displayButtons[i].Disable();
			}
		}

	    private void SetControlsButtonState(bool Enabled){
	        if (Enabled)
	        {
	            this.ResetFonts();
	            _controls.Text.TextFont = _boldFont;
	            this.UpdateFonts();
	        }
	        for (int i = 0; i < _controlsText.ControlsElements.Count; i++){
				if(Enabled)
					_controlsText.ControlsElements[i].Enable();
				else
					_controlsText.ControlsElements[i].Disable();
			}
		}
	}
}
