/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/07/2016
 * Time: 06:34 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using OpenTK;
using Hedra.Engine.Management;
using Hedra.Engine.Player;

namespace Hedra.Engine.Rendering.UI
{
	/// <summary>
	/// Description of GameUI.
	/// </summary>
	public class GameUI : Panel
	{
        public readonly Texture Cross;
	    public readonly Texture QuestLogMsg;
		private readonly Texture _compass;
		private readonly Texture _help;
		private readonly RenderableTexture _classLogo;
	    private readonly RenderableTexture _oxygenBackground;
	    private readonly RenderableTexture _staminaBackground;
	    private readonly RenderableTexture _staminaIcon;
	    private readonly RenderableTexture _oxygenIcon;
		private readonly TexturedBar _oxygenBar;
		private readonly TexturedBar _staminaBar;
		private readonly GUIText _consecutiveHits;
		private readonly SlingShotAnimation _slingShot;
		
		public GameUI(IPlayer Player)
		{
		    _consecutiveHits = new GUIText(string.Empty, new Vector2(0f, -0.75f), Color.Transparent, FontCache.Get(AssetManager.BoldFamily, 1f, FontStyle.Bold));
			_slingShot = new SlingShotAnimation();
		    _slingShot.Play(_consecutiveHits);
            Player.OnHitLanded += delegate
            {
                if (_slingShot.Active) return;
				TaskManager.When(() => _consecutiveHits.Scale.Y > 0, delegate
				{
				    _slingShot.Play(_consecutiveHits);
				});
			};
				
			var barBackgrounds = new RenderableTexture( new Texture(Graphics2D.LoadFromAssets("Assets/UI/BarBackgrounds.png"), Vector2.Zero, Vector2.One), DrawOrder.After);
			_oxygenBackground = new RenderableTexture( new Texture(Graphics2D.LoadFromAssets("Assets/UI/OxygenBackground.png"), Vector2.Zero, Vector2.One), DrawOrder.After);
			_staminaBackground = new RenderableTexture( new Texture(Graphics2D.LoadFromAssets("Assets/UI/StaminaBackground.png"), Vector2.Zero, Vector2.One), DrawOrder.After);
			
			var healthBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/HealthBar.png"), new Vector2(-.675f, .7775f), new Vector2(0.12f, 0.022f),
			    () => Player.Health,
			    () => Player.MaxHealth, this);

			var manaBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/ManaBar.png"), new Vector2(-.7315f, .7265f), new Vector2(0.07f, 0.015f),
			    () => Player.Mana,
			    () => Player.MaxMana, this);

			var xpBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/XPBar.png"), new Vector2(-.675f, .815f), new Vector2(0.12f, 0.0065f),
			    () => Player.XP,
			    () => Player.MaxXP, this);

			_oxygenBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/OxygenBar.png"), new Vector2(-.84f, .6f), new Vector2(0.049f, 0.02f),
			    () => Player.Oxygen,
			    () => Player.MaxOxygen, this);

			_staminaBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/StaminaBar.png"), new Vector2(-.84f, .6f), new Vector2(0.049f, 0.02f),
			    () => Player.Stamina, () => Player.MaxStamina, this);
			
			_classLogo = new RenderableTexture(new Texture(0, Vector2.Zero, Vector2.One), DrawOrder.After);
			_oxygenIcon = new RenderableTexture( new Texture(Graphics2D.LoadFromAssets("Assets/UI/OxygenIcon.png"), Vector2.Zero, Vector2.One), DrawOrder.After);
			_staminaIcon = new RenderableTexture( new Texture(Graphics2D.LoadFromAssets("Assets/UI/StaminaIcon.png"), Vector2.Zero, Vector2.One), DrawOrder.After);
			
			Cross = new Texture("Assets/Pointer.png", new Vector2(0, 0f), Vector2.One * .1f);
			
			var skillTreeMsg = new Texture(Graphics2D.LoadFromAssets("Assets/UI/SkillTreeMsg.png"), Vector2.Zero, Vector2.One);
			QuestLogMsg = new Texture(Graphics2D.LoadFromAssets("Assets/UI/QuestLogMsg.png"), Vector2.Zero, Vector2.One);
			var mapMsg = new Texture(Graphics2D.LoadFromAssets("Assets/UI/MapMsg.png"), Vector2.Zero, Vector2.One);
			
			_compass = new Texture(Graphics2D.LoadFromAssets("Assets/UI/Compass.png"), Vector2.One - new Vector2(0.0366f, 0.065f) * 2f, new Vector2(0.0366f, 0.065f));
			_help = new Texture(Graphics2D.LoadFromAssets("Assets/UI/Help.png"), Vector2.Zero, Vector2.One);
			
            AddElement(_consecutiveHits);
			AddElement(_compass);
			AddElement(barBackgrounds);
			AddElement(xpBar);
			AddElement(_classLogo);
			AddElement(QuestLogMsg);
			AddElement(skillTreeMsg);
			AddElement(Cross);
			AddElement(skillTreeMsg);
			AddElement(_oxygenBar);
			AddElement(_staminaBar);
			AddElement(healthBar);
			AddElement(manaBar);
			AddElement(mapMsg);
			AddElement(_help);
			
			this.OnPanelStateChange += delegate(object Sender, PanelState E)
			{ 
				LocalPlayer.Instance.Minimap.Show = E == PanelState.Enabled;
			    LocalPlayer.Instance.Toolbar.Show = E == PanelState.Enabled;
			};	
		}

		public void Update(IPlayer Player)
		{
            if (Enabled)
			{
				if(Program.GameWindow.FirstLaunch)
				{
					Program.GameWindow.FirstLaunch = false;
					Player.MessageDispatcher.ShowMessageWhile("[F4] HELP", () => !LocalPlayer.Instance.UI.ShowHelp);
				}
			}

			_compass.Disable();
			_compass.TextureElement.Angle = Player.Model.Rotation.Y;
			
			if (Player.UI.ShowHelp && Enabled)
			{
				Player.AbilityTree.Show = false;
				Player.QuestLog.Show = false;
				_help.Enable();
			}
			else
			{
				_help.Disable();
			}

			Oxygen = Math.Abs(Player.Oxygen - Player.MaxOxygen) > 0.05f && !GameSettings.Paused && Enabled;

			Stamina = Math.Abs(Player.Stamina - Player.MaxStamina) > 0.05f && !GameSettings.Paused && Enabled &&
			          !Player.IsUnderwater;

			_classLogo.BaseTexture.TextureElement.TextureId = Player.Class.Logo;
			_consecutiveHits.TextColor = Player.ConsecutiveHits >= 4 && Player.ConsecutiveHits < 8
				? Color.Gold : Player.ConsecutiveHits >= 8 ? Color.Red : Color.White;
			_consecutiveHits.TextFont = FontCache.Get(_consecutiveHits.TextFont.FontFamily,
				Player.ConsecutiveHits >= 4 && Player.ConsecutiveHits < 8
					? 15f : Player.ConsecutiveHits >= 8 ? 17f : 14f,
				_consecutiveHits.TextFont.Style);
			_consecutiveHits.Text = Player.ConsecutiveHits > 0 ? $"{Player.ConsecutiveHits} HIT{(Player.ConsecutiveHits == 1 ? string.Empty : "S")}" : string.Empty;
            _slingShot.Update();
        }
		
		public bool Oxygen
		{
			set{
				if(value){
					_oxygenBar.Enable();
					_oxygenBackground.Enable();
					_oxygenIcon.Enable();
				}else{
					_oxygenBackground.Disable();
					_oxygenBar.Disable();
					_oxygenIcon.Disable();
				}
			}
		}
		
		public bool Stamina
		{
			set{
				if(value){
					_staminaBar.Enable();
					_staminaBackground.Enable();
					_staminaIcon.Enable();
				}else{
					_staminaBackground.Disable();
					_staminaBar.Disable();
					_staminaIcon.Disable();
				}
			}
		}
	}
}
