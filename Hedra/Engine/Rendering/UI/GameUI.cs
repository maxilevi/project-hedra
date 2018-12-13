/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 24/07/2016
 * Time: 06:34 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using Hedra.Engine.Game;
using Hedra.Engine.Localization;
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
        private readonly Texture _compass;
        private readonly Texture _help;
        private readonly RenderableTexture _classLogo;
        private readonly RenderableTexture _oxygenBackground;
        private readonly RenderableTexture _staminaBackground;
        private readonly RenderableTexture _staminaIcon;
        private readonly RenderableTexture _oxygenIcon;
        private readonly RenderableTexture _healthBackground, _xpBackground, _manaBackground;
        private readonly TexturedBar _oxygenBar;
        private readonly TexturedBar _staminaBar;
        private readonly GUIText _consecutiveHits;
        private readonly SlingShotAnimation _slingShot;
        private readonly IPlayer _player;
        private string _currentClass;


        public GameUI(IPlayer Player)
        {
            this._player = Player;
            _consecutiveHits = new GUIText(string.Empty, new Vector2(0f, -0.75f), Color.Transparent, FontCache.Get(AssetManager.BoldFamily, 1f, FontStyle.Bold));
            _slingShot = new SlingShotAnimation();
            _slingShot.Play(_consecutiveHits);
            Player.OnHitLanded += delegate
            {
                if (_slingShot.Active) return;
                TaskScheduler.When(() => _consecutiveHits.Scale.Y > 0, delegate
                {
                    _slingShot.Play(_consecutiveHits);
                });
            };
            _healthBackground = new RenderableTexture(new Texture("Assets/UI/HealthBackground.png", new Vector2(-.675f, .765f), Vector2.One), DrawOrder.After);
            _manaBackground = new RenderableTexture(new Texture("Assets/UI/ManaBackground.png", new Vector2(-.7315f, .7155f), Vector2.One), DrawOrder.After);
            _xpBackground = new RenderableTexture(new Texture("Assets/UI/XPBackground.png", new Vector2(-.675f, .805f), Vector2.One), DrawOrder.After);

            var healthBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/HealthBar.png"), new Vector2(-.675f, .7775f), Graphics2D.SizeFromAssets("Assets/UI/HealthBar.png"),
                () => Player.Health,
                () => Player.MaxHealth, this);

            var manaBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/ManaBar.png"), new Vector2(-.7315f, .7265f), Graphics2D.SizeFromAssets("Assets/UI/ManaBar.png"),
                () => Player.Mana,
                () => Player.MaxMana, this);

            var xpBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/XPBar.png"), new Vector2(-.675f, .815f), Graphics2D.SizeFromAssets("Assets/UI/XPBar.png"),
                () => Player.XP,
                () => Player.MaxXP, this);

            _oxygenBackground = new RenderableTexture(
                new Texture("Assets/UI/OxygenBackground.png", new Vector2(-.84f, .54f), Vector2.One), DrawOrder.After);
            
            _oxygenBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/OxygenBar.png"), new Vector2(-.84f, .55f), Graphics2D.SizeFromAssets("Assets/UI/OxygenBar.png"),
                () => Player.Oxygen,
                () => Player.MaxOxygen, this);    

            _staminaBackground = new RenderableTexture(
                new Texture("Assets/UI/StaminaBackground.png", new Vector2(-.85f, .44f), Vector2.One), DrawOrder.After);
            
            _staminaBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/StaminaBar.png"), new Vector2(-.85f, .45f), Graphics2D.SizeFromAssets("Assets/UI/StaminaBar.png"),
                () => Player.Stamina, () => Player.MaxStamina, this);            

            
            _classLogo = new RenderableTexture(new Texture(0, new Vector2(-.85f, .75f), Vector2.One), DrawOrder.After);
            _oxygenIcon = new RenderableTexture(new Texture("Assets/UI/OxygenIcon.png", new Vector2(-.9f, .54f), Vector2.One), DrawOrder.After);
            _staminaIcon = new RenderableTexture(new Texture("Assets/UI/StaminaIcon.png", new Vector2(-.9f, .44f), Vector2.One), DrawOrder.After);
            
            Cross = new Texture("Assets/UI/Pointer.png", new Vector2(0, 0f), Vector2.One * .1f);
            
            _compass = new Texture(Graphics2D.LoadFromAssets("Assets/UI/Compass.png"), Vector2.One - new Vector2(0.0366f, 0.065f) * 2f, new Vector2(0.0366f, 0.065f));
            _help = new Texture(Graphics2D.LoadFromAssets("Assets/UI/Help.png"), Vector2.Zero, Vector2.One);

            var skillTreeTranslation = Translation.Create("skill_tree_label");
            skillTreeTranslation.Concat(() => $" - {Controls.Skilltree}");
            var skillTreeMsg = new GUIText(skillTreeTranslation, new Vector2(-.85f, -.9f), Color.FromArgb(200, 255, 255, 255), FontCache.Get(AssetManager.BoldFamily, 14));
            var mapTranslation = Translation.Create("map_label");
            skillTreeTranslation.Concat(() => $" - {Controls.Map}");
            var mapMsg = new GUIText(mapTranslation, new Vector2(.815f, .425f), Color.FromArgb(200, 255, 255, 255), FontCache.Get(AssetManager.BoldFamily, 14));
            
            Controls.OnControlsChanged += () =>
            {
                mapTranslation.UpdateTranslation();
                skillTreeTranslation.UpdateTranslation();
            };
            
            AddElement(skillTreeMsg);
            AddElement(mapMsg);
            AddElement(_consecutiveHits);
            AddElement(_compass);
            AddElement(_healthBackground);
            AddElement(_manaBackground);
            AddElement(_xpBackground);
            AddElement(xpBar);
            AddElement(_classLogo);
            AddElement(_staminaIcon);
            AddElement(_oxygenIcon);
            AddElement(Cross);
            AddElement(_oxygenBar);
            AddElement(_staminaBar);
            AddElement(_staminaBackground);
            AddElement(_oxygenBackground);
            AddElement(healthBar);
            AddElement(manaBar);
            AddElement(_help);
            
            this.OnPanelStateChange += delegate(object Sender, PanelState E)
            { 
                LocalPlayer.Instance.Minimap.Show = E == PanelState.Enabled;
                LocalPlayer.Instance.Toolbar.Show = E == PanelState.Enabled;
            };    
        }

        public void Update()
        {
            _compass.Disable();
            _compass.TextureElement.Angle = _player.Model.Rotation.Y;
            
            if (_player.UI.ShowHelp && Enabled)
            {
                _player.AbilityTree.Show = false;
                _player.QuestLog.Show = false;
                _help.Enable();
            }
            else
            {
                _help.Disable();
            }
            
            if(_currentClass != _player.Class.Logo)
                this.UpdateLogo();

            Oxygen = Math.Abs(_player.Oxygen - _player.MaxOxygen) > 0.05f && !GameSettings.Paused && Enabled;

            Stamina = Math.Abs(_player.Stamina - _player.MaxStamina) > 0.05f && !GameSettings.Paused && Enabled &&
                      !_player.IsUnderwater;

            _consecutiveHits.TextColor = _player.ConsecutiveHits >= 4 && _player.ConsecutiveHits < 8
                ? Color.Gold : _player.ConsecutiveHits >= 8 ? Color.Red : Color.White;
            _consecutiveHits.TextFont = FontCache.Get(_consecutiveHits.TextFont.FontFamily,
                _player.ConsecutiveHits >= 4 && _player.ConsecutiveHits < 8
                    ? 15f : _player.ConsecutiveHits >= 8 ? 17f : 14f,
                _consecutiveHits.TextFont.Style);
            var hits = _player.ConsecutiveHits == 1 ? Translations.Get("hit_label") : Translations.Get("hits_label");
            _consecutiveHits.Text = _player.ConsecutiveHits > 0 ? $"{_player.ConsecutiveHits} {hits}" : string.Empty;
            _slingShot.Update();
        }

        private void UpdateLogo()
        {
            _currentClass = _player.Class.Logo;
            _classLogo.BaseTexture.TextureElement.TextureId = Graphics2D.LoadFromAssets(_player.Class.Logo);
            _classLogo.Scale = Graphics2D.SizeFromAssets(_player.Class.Logo);
        }

        private bool Oxygen
        {
            set{
                if(value)
                {
                    _oxygenBar.Enable();
                    _oxygenBackground.Enable();
                    _oxygenIcon.Enable();
                }
                else
                {
                    _oxygenBackground.Disable();
                    _oxygenBar.Disable();
                    _oxygenIcon.Disable();
                }
            }
        }

        private bool Stamina
        {
            set{
                if(value)
                {
                    _staminaBar.Enable();
                    _staminaBackground.Enable();
                    _staminaIcon.Enable();
                }
                else
                {
                    _staminaBackground.Disable();
                    _staminaBar.Disable();
                    _staminaIcon.Disable();
                }
            }
        }
    }
}
