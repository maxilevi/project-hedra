/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/07/2016
 * Time: 12:20 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.EntitySystem.BossSystem
{
    /// <inheritdoc />
    /// <summary>
    ///     Description of BossHealthBarComponent.
    /// </summary>
    internal class BossHealthBarComponent : EntityComponent, IDisposable
    {
        private Vector2 _barDefaultScale;
        private Bar _healthBar;
        private bool _initialized;
        private Vector2 _nameDefaultScale;
        private GUIText _nameGui;
        private float _targetSize;
        public bool Enabled = true;
        public string Name;

        public BossHealthBarComponent(Entity Parent, string Name) : base(Parent)
        {
            this.Name = Name;
        }


        private void Initialize()
        {
            if (_initialized)
                return;
            _initialized = true;

            LocalPlayer player = GameManager.Player;

            if (player == null || _healthBar != null)
                return;

            _healthBar = new Bar(new Vector2(0f, .7f), new Vector2(0.35f, 0.03f), () => Parent.Health,
                () => Parent.MaxHealth,
                player.UI.GamePanel);

            _nameGui = new GUIText(Name, new Vector2(0f, .85f), Color.White,
                FontCache.Get(UserInterface.Fonts.Families[0], 14));


            DrawManager.UIRenderer.Remove(_healthBar.Text);
            DrawManager.UIRenderer.Add(_healthBar.Text, DrawOrder.After);
            _barDefaultScale = _healthBar.Scale;
            _nameDefaultScale = _nameGui.Scale;

            player.UI.GamePanel.AddElement(_nameGui);
            player.UI.GamePanel.AddElement(_healthBar);
        }

        public override void Update()
        {
            this.Initialize();
            if (!Enabled) return;

            LocalPlayer player = GameManager.Player;

            if (player == null) return;

            if (_healthBar == null) return;

            if (_nameGui != null)
            {
                _nameGui.Scale = Mathf.Lerp(_nameGui.Scale, _nameDefaultScale * _targetSize,
                    (float) Time.deltaTime * 8f);
                if(_targetSize > 0)
                    _nameGui.Enable();
                else
                    _nameGui.Disable();
            }


            _healthBar.Scale =
                Mathf.Lerp(_healthBar.Scale, _barDefaultScale * _targetSize, (float) Time.deltaTime * 8f);
            //_healthBar.Text.Text = Parent.Health + "/" + Parent.MaxHealth;

            if (_healthBar.Scale.LengthSquared < new Vector2(0.002f, 0.002f).LengthSquared)
                _healthBar.Scale = Vector2.Zero;

            if (player.UI.GamePanel.Enabled)
                _healthBar.Enable();
            else
                _healthBar.Disable();

            if (_healthBar.Scale == Vector2.Zero)
                _healthBar.Disable();

            if (player.UI.GamePanel.Enabled && (Parent.Position - player.Position).LengthSquared < 14400)
            {
                _targetSize = 1;
                _healthBar.Text.Enable();
            }
            else
            {
                _targetSize = 0;
                _healthBar.Text.Disable();
            }
        }

        public override void Draw() { }

        public override void Dispose()
        {
            this.Initialize();
            LocalPlayer player = GameManager.Player;
            player.UI.GamePanel.RemoveElement(_healthBar);
            player.UI.GamePanel.RemoveElement(_nameGui);
            _healthBar.Dispose();
            _nameGui.Dispose();
        }
    }
}