using System;
using System.Drawing;
using System.Globalization;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Frustum;
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
        private static readonly Color Friendly = Color.YellowGreen;
        private static readonly Color Hostile = Color.Red;
        private static readonly Color Immune = Color.CornflowerBlue;
        private static readonly Color Neutral = Color.White;

        private static uint _friendlyTexture;
        private static uint _hostileTexture;
        private static uint _immuneTexture;
        
        private readonly TexturedBar _healthBar;
        private readonly RenderableText _text;
        private readonly Vector2 _originalScale = new Vector2(0.06f, 0.025f) * .65f;
        private Vector2 _originalTextScale;
        private float _barSize;
        private string _name;
        private bool _show;
        private float _targetBarSize = 1;
        private float _textEnabled;
        private bool _textUpdated;

        static HealthBarComponent()
        {
            Executer.ExecuteOnMainThread(() =>
            {
                var blueprint = Graphics2D.LoadBitmapFromAssets("Assets/UI/EntityHealthBar.png");
                _friendlyTexture = Graphics2D.LoadTexture(new BitmapObject
                    {
                        Bitmap = Graphics2D.ReplaceColor(
                            Graphics2D.ReplaceColor((Bitmap) blueprint.Clone(), Color.FromArgb(255, 0, 0, 0), Friendly),
                            Color.FromArgb(255, 255, 255, 255), new Vector4(Friendly.ToVector4().Xyz * .75f, 1).ToColor()
                        ),
                        Path = $"UI:Color:HealthBarComponent:Friendly"
                    }
                );
                _hostileTexture = Graphics2D.LoadTexture(new BitmapObject
                    {
                        Bitmap = Graphics2D.ReplaceColor(
                            Graphics2D.ReplaceColor((Bitmap) blueprint.Clone(), Color.FromArgb(255, 0, 0, 0), Hostile),
                            Color.FromArgb(255, 255, 255, 255), new Vector4(Hostile.ToVector4().Xyz * .75f, 1).ToColor()
                        ),
                        Path = $"UI:Color:HealthBarComponent:Hostile"
                    }
                );
                _immuneTexture = Graphics2D.LoadTexture(new BitmapObject
                    {
                        Bitmap = Graphics2D.ReplaceColor(
                            Graphics2D.ReplaceColor((Bitmap) blueprint.Clone(), Color.FromArgb(255, 0, 0, 0), Immune),
                            Color.FromArgb(255, 255, 255, 255), new Vector4(Immune.ToVector4().Xyz * .75f, 1).ToColor()
                        ),
                        Path = $"UI:Color:HealthBarComponent:Immune"
                    }
                );
            });
        }
        
        public HealthBarComponent(IEntity Parent, string Name, HealthBarType Type) : this(Parent, Name, Type, Neutral)
        {    
        }
        
        public HealthBarComponent(IEntity Parent, string Name, HealthBarType Type, Color FontColor) : base(Parent)
        {
            var color = ColorFromType(Type);
            _healthBar = new TexturedBar(
                0,
                Vector2.Zero,
                Mathf.ScaleGui(new Vector2(1024, 578), _originalScale),
                () => Parent.Health,
                () => Parent.MaxHealth
            );
            Executer.ExecuteOnMainThread(
                () => _healthBar.TextureId = TextureFromType(Type)
            );
            _text = new RenderableText(string.Empty, Vector2.Zero, FontColor, FontCache.Get(AssetManager.BoldFamily, 11, FontStyle.Bold));
            this.Name = Name;
            DrawManager.UIRenderer.Remove(_text);
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

            _barSize = Mathf.Lerp(_barSize, _targetBarSize, Time.DeltaTime * 16f);
            _text.Scale = _originalTextScale * _barSize * _textEnabled;

            var product = 
                Vector3.Dot(GameManager.Player.View.CrossDirection, (Parent.Position - GameManager.Player.Position).NormalizedFast());
            if (_barSize <= 0.5f || product <= 0.0f)
            {
                _healthBar.Disable();
                _text.Disable();
                _targetBarSize = 0;
                _textEnabled = 0; 
            }
            else
            {
                _healthBar.Enable();
                _text.Enable();
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

            var eyeSpace = Vector4.Transform(new Vector4(Parent.Position + Parent.Model.Height * 1.5f * Vector3.UnitY, 1),
                    Culling.ModelViewMatrix);
            var homogeneousSpace = Vector4.Transform(eyeSpace, Culling.ProjectionMatrix);
            var ndc = homogeneousSpace.Xyz / homogeneousSpace.W;
            _healthBar.Position = Mathf.Clamp(ndc.Xy, -.98f, .98f) + (1 - _barSize) * _originalScale.X * Vector2.UnitX;
            _healthBar.Scale = _originalScale * _barSize * Math.Min(1, Parent.Model.Height / 7f);
            _text.Position = _healthBar.Position + Vector2.UnitY * _originalScale * 3f - (1 - _barSize) * _originalScale.X * Vector2.UnitX;
            _healthBar.Draw();
            _text.Draw();
        }

        private static Color ColorFromType(HealthBarType Type)
        {
            return Type == HealthBarType.Friendly
                ? Friendly
                : Type == HealthBarType.Hostile
                    ? Hostile
                    : Type == HealthBarType.Immune
                        ? Immune
                        : Neutral;
        }
        
        private static uint TextureFromType(HealthBarType Type)
        {
            return Type == HealthBarType.Friendly
                ? _friendlyTexture
                : Type == HealthBarType.Hostile
                    ? _hostileTexture
                    : Type == HealthBarType.Immune
                        ? _immuneTexture
                        : throw new ArgumentOutOfRangeException($"Texture for type '{Type}' doesn't exists.");
        }
        
        public bool Hide { get; set; }
        
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                _text.Text = _name;
                _originalTextScale = _text.Scale;
            }
        }
    }
    
    public enum HealthBarType
    {
        Hostile,
        Immune,
        Friendly,
        Neutral
    }
}