using System;
using System.Drawing;
using System.Globalization;
using Hedra.Engine.Game;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Scenes;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.EntitySystem
{
    /// <inheritdoc cref="EntityComponent" />
    /// <summary>
    ///     Description of HealthComponent.
    /// </summary>
    public class HealthBarComponent : EntityComponent, IRenderable, IDisposable
    {
        private static readonly Panel HealthBarPanel = new Panel();
        private readonly Bar _healthBar;
        private readonly Vector2 _originalScale = new Vector2(0.075f, 0.025f) * .8f;
        private readonly Vector2 _originalTextScale;
        private float _barSize;
        private string _name;
        private bool _show;
        private float _targetBarSize = 1;
        private float _textEnabled;
        private bool _textUpdated;
        public Color FontColor = Color.White;

        public bool Hide { get; set; }

        public string Name
        {
            get => _name ?? Parent.Type.AddSpacesToSentence(true);
            set
            {
                _name = value;
                if (_healthBar?.Text != null) (_healthBar?.Text).Text = _name;
            }
        }

        public HealthBarComponent(IEntity Parent, string Name) : base(Parent)
        {
            _healthBar = new Bar(Vector2.Zero, Mathf.ScaleGUI(new Vector2(1024, 578), _originalScale), Name,
                () => Parent.Health, () => Parent.MaxHealth,
                HealthBarPanel);

            _name = Name;
            _originalTextScale = _healthBar.Text.UIText.UIText.Scale;
            _healthBar.UpdateTextRatio = false;

            DrawManager.UIRenderer.Remove(_healthBar);
            DrawManager.UIRenderer.Add(this, DrawOrder.After);
        }

        public HealthBarComponent(IEntity Parent) : base(Parent)
        {
            _healthBar = new Bar(Vector2.Zero, Mathf.ScaleGUI(new Vector2(1024, 578), _originalScale), Name,
                () => Parent.Health, () => Parent.MaxHealth,
                HealthBarPanel);

            _healthBar.UpdateTextRatio = false;
            _originalTextScale = _healthBar.Text.UIText.UIText.Scale;

            DrawManager.UIRenderer.Remove(_healthBar);
            DrawManager.UIRenderer.Add(this, DrawOrder.After);
        }

        public override void Update()
        {
            _show = (Parent.Model.Position.Xz - GameManager.Player.Position.Xz).LengthSquared < 45 * 45 
                && !Hide
                && !Parent.IsDead
                && !GameSettings.Paused
                && !GameManager.IsLoading;

            _targetBarSize = _show ? 1 : 0;

            _barSize = Mathf.Lerp(_barSize, _targetBarSize, (float)Time.DeltaTime * 16f);
            _healthBar.Text.UIText.UIText.Scale = _originalTextScale * _barSize * _textEnabled;

            var product = 
                Mathf.DotProduct(GameManager.Player.View.CrossDirection, (Parent.Position - GameManager.Player.Position).NormalizedFast());
            if (_barSize <= 0.5f || product <= 0.5f)
            {
                _healthBar.Disable();
                _targetBarSize = 0;
                _textEnabled = 0; 
            }
            else
            {
                _healthBar.Enable();
                _targetBarSize = 1;
                _textEnabled = 1;
            }
        }

        public override void Dispose()
        {
            DrawManager.UIRenderer.Remove(this);
            _healthBar.Dispose();
        }

        public override void Draw()
        {
            if(Parent.Model == null) return;

            Vector4 eyeSpace = Vector4.Transform(new Vector4(Parent.Position + Parent.Model.Height * 1.5f * Vector3.UnitY, 1),
                    DrawManager.FrustumObject.ModelViewMatrix);
            Vector4 homogeneusSpace = Vector4.Transform(eyeSpace, DrawManager.FrustumObject.ProjectionMatrix);
            Vector3 ndc = homogeneusSpace.Xyz / homogeneusSpace.W;
            _healthBar.Position = Mathf.Clamp(ndc.Xy, -.98f, .98f);
            _healthBar.Scale = _originalScale * _barSize * Math.Min(1, Parent.Model.Height / 7f);

            if (!_textUpdated)
            {
                _healthBar.Text.UIText.TextColor = FontColor;
                _healthBar.Text.UIText.UpdateText();
                _textUpdated = true;
            }
            _healthBar.CurvedBorders = true;
            _healthBar.Text.Position = _healthBar.Position + Vector2.UnitY * _originalScale * 4;
            _healthBar.Draw();
        }
    }
}