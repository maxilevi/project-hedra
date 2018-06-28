/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/07/2016
 * Time: 06:34 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System.Drawing;
using OpenTK;
using Hedra.Engine.Management;
using Hedra.Engine.Player;

namespace Hedra.Engine.Rendering.UI
{
	/// <summary>
	/// Description of GameUI.
	/// </summary>
	internal class GameUI : Panel
	{
	    public static Vector2 TargetResolution = new Vector2(1024, 578);
        public readonly Texture Cross;
	    public readonly Texture QuestLogMsg;
	    public readonly Texture SkillTreeMsg;
	    public readonly Texture Compass;
	    public readonly Texture MapMsg;
	    public readonly Texture Help;
	    public readonly RenderableTexture ClassLogo;
	    private readonly RenderableTexture _oxygenBackground;
	    private readonly RenderableTexture _staminaBackground;
	    private readonly RenderableTexture _staminaIcon;
	    private readonly RenderableTexture _oxygenIcon;
	    public readonly TexturedBar OxygenBar;
	    public readonly TexturedBar StaminaBar;
	    public readonly GUIText ConsecutiveHits;
		
		public GameUI(LocalPlayer Player) : base()
		{
		    ConsecutiveHits = new GUIText(string.Empty, new Vector2(0f, -0.75f), Color.Transparent, FontCache.Get(AssetManager.BoldFamily, 1f, FontStyle.Bold));
            RenderableTexture barBackgrounds = new RenderableTexture( new Texture(Graphics2D.LoadFromAssets("Assets/UI/BarBackgrounds.png"), Vector2.Zero, Vector2.One), DrawOrder.After);
			_oxygenBackground = new RenderableTexture( new Texture(Graphics2D.LoadFromAssets("Assets/UI/OxygenBackground.png"), Vector2.Zero, Vector2.One), DrawOrder.After);
			_staminaBackground = new RenderableTexture( new Texture(Graphics2D.LoadFromAssets("Assets/UI/StaminaBackground.png"), Vector2.Zero, Vector2.One), DrawOrder.After);
			
			var healthBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/HealthBar.png"), new Vector2(-.675f, .7775f), new Vector2(0.12f, 0.022f),
			    () => Player.Health,
			    () => Player.MaxHealth, this);

			var manaBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/ManaBar.png"), new Vector2(-.7315f, .7265f), new Vector2(0.07f, 0.015f),
			    () => Player.Mana,
			    () => Player.MaxMana, this);

			TexturedBar xpBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/XPBar.png"), new Vector2(-.675f, .815f), new Vector2(0.12f, 0.0065f),
			    () => Player.XP,
			    () => Player.MaxXP, this);

			OxygenBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/OxygenBar.png"), new Vector2(-.84f, .6f), new Vector2(0.049f, 0.02f),
			    () => Player.Oxygen,
			    () => Player.MaxOxygen, this);

			StaminaBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/StaminaBar.png"), new Vector2(-.84f, .6f), new Vector2(0.049f, 0.02f),
			    () => Player.Stamina, () => Player.MaxStamina, this);
			
			ClassLogo = new RenderableTexture(new Texture(0, Vector2.Zero, Vector2.One), DrawOrder.After);
			_oxygenIcon = new RenderableTexture( new Texture(Graphics2D.LoadFromAssets("Assets/UI/OxygenIcon.png"), Vector2.Zero, Vector2.One), DrawOrder.After);
			_staminaIcon = new RenderableTexture( new Texture(Graphics2D.LoadFromAssets("Assets/UI/StaminaIcon.png"), Vector2.Zero, Vector2.One), DrawOrder.After);
			
			Cross = new Texture("Assets/Pointer.png", new Vector2(0, 0f), Vector2.One * .1f);
			
			SkillTreeMsg = new Texture(Graphics2D.LoadFromAssets("Assets/UI/SkillTreeMsg.png"), Vector2.Zero, Vector2.One);
			QuestLogMsg = new Texture(Graphics2D.LoadFromAssets("Assets/UI/QuestLogMsg.png"), Vector2.Zero, Vector2.One);
			MapMsg = new Texture(Graphics2D.LoadFromAssets("Assets/UI/MapMsg.png"), Vector2.Zero, Vector2.One);
			
			Compass = new Texture(Graphics2D.LoadFromAssets("Assets/UI/Compass.png"), Vector2.One - new Vector2(0.0366f, 0.065f) * 2f, new Vector2(0.0366f, 0.065f));
			Help = new Texture(Graphics2D.LoadFromAssets("Assets/UI/Help.png"), Vector2.Zero, Vector2.One);
			
            this.AddElement(ConsecutiveHits);
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
			this.AddElement(healthBar);
			this.AddElement(manaBar);
			this.AddElement(MapMsg);
			this.AddElement(Help);
			
			this.OnPanelStateChange += delegate(object Sender, PanelState E) { 
				LocalPlayer.Instance.Minimap.Show = E == PanelState.Enabled;
			    LocalPlayer.Instance.Toolbar.Show = E == PanelState.Enabled;
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
