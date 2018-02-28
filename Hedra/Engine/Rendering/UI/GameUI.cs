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
using Hedra.Engine.Events;

namespace Hedra.Engine.Rendering.UI
{
	/// <summary>
	/// Description of GameUI.
	/// </summary>
	public class GameUI : Panel
	{
		public static uint WarriorLogo = Graphics2D.LoadFromAssets("Assets/UI/WarriorLogo.png"),
						   ArcherLogo = Graphics2D.LoadFromAssets("Assets/UI/ArcherLogo.png"),
						   RogueLogo = Graphics2D.LoadFromAssets("Assets/UI/RogueLogo.png");
		public Texture Cross, QuestLogMsg, SkillTreeMsg, Compass, MapMsg, Help;
		public RenderableTexture ClassLogo;
		private RenderableTexture _oxygenBackground, _staminaBackground, _staminaIcon, _oxygenIcon;
		private LocalPlayer _player;
		public static Vector2 TargetResolution = new Vector2(1024,578);
		private TexturedBar _healthBar, _manaBar;
		public TexturedBar OxygenBar, StaminaBar;
		//public GUIText 
		
		public GameUI(LocalPlayer Player) : base()
		{
			this._player = Player;
			
			RenderableTexture barBackgrounds = new RenderableTexture( new Texture(Graphics2D.LoadFromAssets("Assets/UI/BarBackgrounds.png"), Vector2.Zero, Vector2.One), false );
			_oxygenBackground = new RenderableTexture( new Texture(Graphics2D.LoadFromAssets("Assets/UI/OxygenBackground.png"), Vector2.Zero, Vector2.One), false );
			_staminaBackground = new RenderableTexture( new Texture(Graphics2D.LoadFromAssets("Assets/UI/StaminaBackground.png"), Vector2.Zero, Vector2.One), false );
			
			_healthBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/HealthBar.png"), new Vector2(-.675f, .7775f), new Vector2(0.12f, 0.022f), delegate{ return Player.Health; } , delegate{ return Player.MaxHealth; }, this);
			_manaBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/ManaBar.png"), new Vector2(-.7315f, .7265f), new Vector2(0.07f, 0.015f), delegate{ return Player.Mana; }, delegate{ return Player.MaxMana; }, this);
			TexturedBar xpBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/XPBar.png"), new Vector2(-.675f, .815f), new Vector2(0.12f, 0.0065f), delegate{ return Player.XP; }, delegate{ return Player.MaxXP; }, this);
			
			OxygenBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/OxygenBar.png"), new Vector2(-.84f, .6f), new Vector2(0.049f, 0.02f), delegate{ return Player.Oxygen; }, delegate{ return Player.MaxOxygen; }, this);
			StaminaBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/StaminaBar.png"), new Vector2(-.84f, .6f), new Vector2(0.049f, 0.02f), delegate{ return Player.Stamina; }, delegate{ return Player.MaxStamina; }, this);
			
			ClassLogo = new RenderableTexture(new Texture(0, Vector2.Zero, Vector2.One), false);
			_oxygenIcon = new RenderableTexture( new Texture(Graphics2D.LoadFromAssets("Assets/UI/OxygenIcon.png"), Vector2.Zero, Vector2.One), false );
			_staminaIcon = new RenderableTexture( new Texture(Graphics2D.LoadFromAssets("Assets/UI/StaminaIcon.png"), Vector2.Zero, Vector2.One), false );
			
			Cross = new Texture(Graphics2D.LoadFromAssets("Assets/Pointer.png"), new Vector2(0, 0f), new Vector2(0.018f, 0.026f) );
			
			SkillTreeMsg = new Texture(Graphics2D.LoadFromAssets("Assets/UI/SkillTreeMsg.png"), Vector2.Zero, Vector2.One);
			QuestLogMsg = new Texture(Graphics2D.LoadFromAssets("Assets/UI/QuestLogMsg.png"), Vector2.Zero, Vector2.One);
			MapMsg = new Texture(Graphics2D.LoadFromAssets("Assets/UI/MapMsg.png"), Vector2.Zero, Vector2.One);
			
			Compass = new Texture(Graphics2D.LoadFromAssets("Assets/UI/Compass.png"), Vector2.One - new Vector2(0.0366f, 0.065f) * 2f, new Vector2(0.0366f, 0.065f));
			Help = new Texture(Graphics2D.LoadFromAssets("Assets/UI/Help.png"), Vector2.Zero, Vector2.One);
			
			this.AddElement(Compass);
			this.AddElement(barBackgrounds);
			this.AddElement(xpBar);
			this.AddElement(ClassLogo);
			this.AddElement(QuestLogMsg);
			this.AddElement(SkillTreeMsg);
			this.AddElement(Cross);
			this.AddElement(SkillTreeMsg);
			this.AddElement(OxygenBar);
			this.AddElement(StaminaBar);
			this.AddElement(_healthBar);
			this.AddElement(_manaBar);
			this.AddElement(MapMsg);
			this.AddElement(Help);
			
			this.OnPanelStateChange += delegate(object Sender, PanelState E) { 
				LocalPlayer.Instance.Minimap.Show = E == PanelState.Enabled;
			};
			
		}
		
		public bool Oxygen{
			set{
				if(value){
					OxygenBar.Enable();
					_oxygenBackground.Enable();
					_oxygenIcon.Enable();
				}else{
					_oxygenBackground.Disable();
					OxygenBar.Disable();
					_oxygenIcon.Disable();
				}
			}
		}
		
		public bool Stamina{
			set{
				if(value){
					StaminaBar.Enable();
					_staminaBackground.Enable();
					_staminaIcon.Enable();
				}else{
					_staminaBackground.Disable();
					StaminaBar.Disable();
					_staminaIcon.Disable();
				}
			}
		}
	}
}
