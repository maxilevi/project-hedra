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
using Hedra.Game;
using Hedra.Localization;
using Hedra.Rendering.UI;

namespace Hedra.Engine.Rendering.UI
{
    /// <summary>
    /// Description of GameUI.
    /// </summary>
    public class GameUI : Panel
    {
        public readonly BackgroundTexture Cross;
        private readonly BackgroundTexture _compass;
        private readonly RenderableTexture _classLogo;
        private readonly RenderableTexture _oxygenBackground;
        private readonly RenderableTexture _staminaBackground;
        private readonly RenderableTexture _staminaIcon;
        private readonly RenderableTexture _oxygenIcon;
        private readonly BackgroundTexture _healthBackground, _xpBackground, _manaBackground;
        private readonly TexturedBar _healthBar;
        private readonly TexturedBar _manaBar;
        private readonly TexturedBar _xpBar;
        private readonly TexturedBar _oxygenBar;
        private readonly TexturedBar _staminaBar;
        private readonly GUIText _consecutiveHits;
        private readonly SlingShotAnimation _slingShot;
        private readonly IPlayer _player;
        private string _currentClass;
        private bool _shouldPlay;

        public Vector2 LogoPosition => _classLogo.Position;
        
        public GameUI(IPlayer Player)
        {
            _player = Player;
            _consecutiveHits = new GUIText(string.Empty, new Vector2(0f, -0.7f), Color.Transparent, FontCache.GetBold(1f));
            _slingShot = new SlingShotAnimation();
            _slingShot.Play(_consecutiveHits);
            Player.OnHitLanded += delegate
            {
                if (_slingShot.Active) return;
                _shouldPlay = true;
            };
            const float scale = .75f;
            _healthBackground = new BackgroundTexture(
                Graphics2D.LoadFromAssets("Assets/UI/HealthBarBackground.png"), new Vector2(0, .765f), Graphics2D.SizeFromAssets("Assets/UI/HealthBarBackground.png").As1920x1080() * scale
            );
            _manaBackground = new BackgroundTexture(
                Graphics2D.LoadFromAssets("Assets/UI/ManaBarBackground.png"), new Vector2(0, .7155f), Graphics2D.SizeFromAssets("Assets/UI/ManaBarBackground.png").As1920x1080()  * scale
            );
            _xpBackground = new BackgroundTexture(
                Graphics2D.LoadFromAssets("Assets/UI/XpBarBackground.png"), new Vector2(0, .805f), Graphics2D.SizeFromAssets("Assets/UI/XpBarBackground.png").As1920x1080()  * scale
            );

            _healthBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/HealthBar.png"), Vector2.Zero, Graphics2D.SizeFromAssets("Assets/UI/HealthBar.png").As1920x1080()  * scale,
                () => Player.Health,
                () => Player.MaxHealth);

            _manaBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/ManaBar.png"), Vector2.Zero, Graphics2D.SizeFromAssets("Assets/UI/ManaBar.png").As1920x1080()  * scale,
                () => Player.Mana,
                () => Player.MaxMana);

            _xpBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/XpBar.png"), Vector2.Zero, Graphics2D.SizeFromAssets("Assets/UI/XpBar.png").As1920x1080() * scale,
                () => Player.XP,
                () => Player.MaxXP);

            var oxygenAndStaminaBackgroundId = Graphics2D.LoadFromAssets("Assets/UI/StaminaAndOxygenBarBackground.png");
            var oxygenAndStaminaBackgroundSize = Graphics2D.SizeFromAssets("Assets/UI/StaminaAndOxygenBarBackground.png").As1920x1080() * scale;
            
            _oxygenBackground = new RenderableTexture(
                new BackgroundTexture(oxygenAndStaminaBackgroundId, Vector2.Zero, oxygenAndStaminaBackgroundSize),
                DrawOrder.After
            );
            
            _oxygenBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/OxygenBar.png"), Vector2.Zero, Graphics2D.SizeFromAssets("Assets/UI/StaminaBar.png").As1920x1080() * scale,
                () => Player.Oxygen,
                () => Player.MaxOxygen);

            _staminaBackground = new RenderableTexture(
                new BackgroundTexture(oxygenAndStaminaBackgroundId, Vector2.Zero, oxygenAndStaminaBackgroundSize),
                DrawOrder.After
            );
            
            _staminaBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/StaminaBar.png"), Vector2.Zero, Graphics2D.SizeFromAssets("Assets/UI/StaminaBar.png").As1920x1080() * scale,
                () => Player.Stamina, () => Player.MaxStamina);
      
            _classLogo = new RenderableTexture(new BackgroundTexture(0, new Vector2(-.85f, .75f), Vector2.One), DrawOrder.After);
            _oxygenIcon = new RenderableTexture(
                new BackgroundTexture(Graphics2D.LoadFromAssets("Assets/UI/OxygenIcon.png"), Vector2.Zero, Graphics2D.SizeFromAssets("Assets/UI/OxygenIcon.png").As1920x1080() * scale),
                DrawOrder.After
            );
            _staminaIcon = new RenderableTexture(
                new BackgroundTexture(Graphics2D.LoadFromAssets("Assets/UI/StaminaIcon.png"), Vector2.Zero, Graphics2D.SizeFromAssets("Assets/UI/StaminaIcon.png").As1920x1080() * scale),
                DrawOrder.After
            );
            
            Cross = new BackgroundTexture("Assets/UI/Pointer.png", new Vector2(0, 0), Vector2.One * .1f);
            
            _compass = new BackgroundTexture(Graphics2D.LoadFromAssets("Assets/UI/Compass.png"), Vector2.One - new Vector2(0.0366f, 0.065f) * 2f, new Vector2(0.0366f, 0.065f));

            var skillTreeTranslation = Translation.Create("skill_tree_label");
            skillTreeTranslation.Concat(() => $" - {Controls.Skilltree}");
            var skillTreeMsg = new GUIText(skillTreeTranslation, new Vector2(-.85f, -.9f), Color.FromArgb(200, 255, 255, 255), FontCache.GetBold(14));
            
            var questLogTranslation = Translation.Create("quest_log_label");
            questLogTranslation.Concat(() => $" - {Controls.QuestLog}");
            var questLogMsg = new GUIText(questLogTranslation, new Vector2(.85f, -.9f), Color.FromArgb(200, 255, 255, 255), FontCache.GetBold(14));
            
            Controls.OnControlsChanged += () =>
            {
                questLogTranslation.UpdateTranslation();
                skillTreeTranslation.UpdateTranslation();
            };
            
            AddElement(questLogMsg);
            AddElement(skillTreeMsg);
            AddElement(_consecutiveHits);
            AddElement(_compass);
            AddElement(_healthBackground);
            AddElement(_manaBackground);
            AddElement(_xpBackground);
            AddElement(_xpBar);
            AddElement(_classLogo);
            AddElement(_staminaIcon);
            AddElement(_oxygenIcon);
            AddElement(Cross);
            AddElement(_oxygenBar);
            AddElement(_staminaBar);
            AddElement(_staminaBackground);
            AddElement(_oxygenBackground);
            AddElement(_healthBar);
            AddElement(_manaBar);
            
            OnPanelStateChange += delegate(object Sender, PanelState E)
            { 
                LocalPlayer.Instance.Minimap.Show = E == PanelState.Enabled;
                LocalPlayer.Instance.Toolbar.Show = E == PanelState.Enabled;
            };    
        }

        public void Update()
        {
            _compass.Disable();
            _compass.TextureElement.Angle = _player.Model.LocalRotation.Y;

            _healthBackground.Position = new Vector2(_classLogo.Position.X + _healthBackground.Scale.X * 1.5f, _healthBackground.Position.Y);
            _manaBackground.Position = new Vector2(_classLogo.Position.X + _manaBackground.Scale.X * 1.5f, _manaBackground.Position.Y);
            _xpBackground.Position = new Vector2(_classLogo.Position.X + _xpBackground.Scale.X * 1.5f, _xpBackground.Position.Y);
            _staminaBackground.Position = _classLogo.Position - Vector2.UnitY * _classLogo.Scale * 1.5f;
            _oxygenBackground.Position = _classLogo.Position - Vector2.UnitY * _classLogo.Scale * 2.25f;
            
            _healthBar.Position = new Vector2(_classLogo.Position.X + _healthBar.Scale.X * 1.5f, _healthBackground.Position.Y);
            _manaBar.Position = new Vector2(_classLogo.Position.X + _manaBar.Scale.X * 1.5f, _manaBackground.Position.Y);
            _xpBar.Position = new Vector2(_classLogo.Position.X + _xpBar.Scale.X * 1.5f, _xpBackground.Position.Y);
            _staminaBar.Position = _staminaBackground.Position + Vector2.UnitX * (_staminaBackground.Scale.X -_staminaBar.Scale.X) * .5f;
            _oxygenBar.Position = _oxygenBackground.Position + Vector2.UnitX * (_oxygenBackground.Scale.X - _oxygenBar.Scale.X) * .5f;
            _staminaIcon.Position = _staminaBackground.Position;
            _oxygenIcon.Position = _oxygenBackground.Position;
            
            if(_currentClass != _player.Class.Logo)
                this.UpdateLogo();

            Oxygen = Math.Abs(_player.Oxygen - _player.MaxOxygen) > 0.05f && !GameSettings.Paused && Enabled;

            Stamina = Math.Abs(_player.Stamina - _player.MaxStamina) > 0.05f && !GameSettings.Paused && Enabled;

            _consecutiveHits.TextColor = _player.ConsecutiveHits >= 4 && _player.ConsecutiveHits < 8
                ? Color.Gold : _player.ConsecutiveHits >= 8 ? Color.Red : Color.White;
            _consecutiveHits.TextFont = 
                FontCache.Get(_consecutiveHits.TextFont, _player.ConsecutiveHits >= 4 && _player.ConsecutiveHits < 8 ? 15f : _player.ConsecutiveHits >= 8 ? 17f : 14f);
            var hits = _player.ConsecutiveHits == 1 ? Translations.Get("hit_label") : Translations.Get("hits_label");
            _consecutiveHits.Text = _player.ConsecutiveHits > 0 ? $"{_player.ConsecutiveHits} {hits}" : string.Empty;
            if (_shouldPlay)
            {
                _slingShot.Play(_consecutiveHits);
                _shouldPlay = false;
            }
            _slingShot.Update();
        }

        private void UpdateLogo()
        {
            _currentClass = _player.Class.Logo;
            _classLogo.BaseTexture.TextureElement.TextureId = Graphics2D.LoadFromAssets(_player.Class.Logo);
            _classLogo.Scale = Graphics2D.SizeFromAssets(_player.Class.Logo).As1920x1080();
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
