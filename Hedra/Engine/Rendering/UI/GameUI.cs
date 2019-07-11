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
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Game;
using Hedra.Engine.Localization;
using OpenTK;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Player.QuestSystem;
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
        private readonly ProfileUIElement _playerProfile;
        private readonly CompanionProfileUIElement _companionProfile;
        public readonly BackgroundTexture Cross;
        private readonly BackgroundTexture _compass;
        private readonly RenderableTexture _oxygenBackground;
        private readonly RenderableTexture _staminaBackground;
        private readonly RenderableTexture _staminaIcon;
        private readonly RenderableTexture _oxygenIcon;
        private readonly TexturedBar _oxygenBar;
        private readonly TexturedBar _staminaBar;
        private readonly GUIText _consecutiveHits;
        private readonly SlingShotAnimation _slingShot;
        private readonly IPlayer _player;
        private string _currentClass;
        private bool _shouldPlay;

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

            _playerProfile = new ProfileUIElement(Player, new Vector2(-.85f, .75f), scale);
            _companionProfile = new CompanionProfileUIElement(Player, new Vector2(-.85f, .5f), scale * .85f);
            
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
            
            AddElement(_companionProfile);
            AddElement(_playerProfile);
            AddElement(questLogMsg);
            AddElement(skillTreeMsg);
            AddElement(_consecutiveHits);
            AddElement(_compass);
            AddElement(_staminaIcon);
            AddElement(_oxygenIcon);
            AddElement(Cross);
            AddElement(_oxygenBar);
            AddElement(_staminaBar);
            AddElement(_staminaBackground);
            AddElement(_oxygenBackground);

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

            _playerProfile.Update();
            _companionProfile.Update();

            _staminaBackground.Position = _playerProfile.ClassLogo.Position - Vector2.UnitY * _playerProfile.ClassLogo.Scale * 1.5f;
            _oxygenBackground.Position = _playerProfile.ClassLogo.Position - Vector2.UnitY * _playerProfile.ClassLogo.Scale * 2.25f;
            
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
            _playerProfile.ClassLogo.BaseTexture.TextureElement.TextureId = Graphics2D.LoadFromAssets(_player.Class.Logo);
            _playerProfile.ClassLogo.Scale = Graphics2D.SizeFromAssets(_player.Class.Logo).As1920x1080();
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
