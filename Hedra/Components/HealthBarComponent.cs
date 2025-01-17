using System;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.Frustum;
using Hedra.Engine.Rendering.UI;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Hedra.Components
{
    /// <inheritdoc cref="EntityComponent" />
    /// <summary>
    ///     Description of HealthComponent.
    /// </summary>
    public class HealthBarComponent : BaseHealthBarComponent, IRenderable, INamedHealthBar
    {
        private const int ShowDistance = 48;
        private static uint _neutralTexture;
        private static uint _hostileTexture;
        private static uint _friendlyTexture;
        private static uint _blackTexture;
        private static uint _goldTexture;
        private static uint _backgroundTextureId;
        private static Vector2 _backgroundTextureSize;
        private static Vector2 _textureSize;
        private readonly RenderableTexture _backgroundTexture;
        private readonly Vector2 _barDefaultPosition;
        private readonly TexturedBar _healthBar;
        private readonly Panel _panel;
        private readonly RenderableText _text;
        private float _barSize;
        private float _lastProduct;
        private string _name;
        private Vector2 _originalTextScale;
        private bool _show;
        private float _targetBarSize = 1;
        private float _textEnabled;

        static HealthBarComponent()
        {
            Executer.ExecuteOnMainThread(() =>
            {
                var blueprint = Graphics2D.LoadBitmapFromAssets("Assets/UI/EntityHealthBar.png");
                _neutralTexture = BuildTexture(
                    blueprint.CloneAs<Rgba32>(),
                    Friendly,
                    "Neutral"
                );
                _hostileTexture = BuildTexture(
                    blueprint.CloneAs<Rgba32>(),
                    Hostile,
                    "Hostile"
                );
                _friendlyTexture = BuildTexture(
                    blueprint.CloneAs<Rgba32>(),
                    Friendly,
                    "Friendly"
                );
                _blackTexture = BuildTexture(
                    blueprint.CloneAs<Rgba32>(),
                    Color.FromRgba(35, 35, 35, 255),
                    "Black"
                );
                _goldTexture = BuildTexture(
                    blueprint.CloneAs<Rgba32>(),
                    Color.Gold,
                    "Gold"
                );
                blueprint.Dispose();

                _textureSize = Graphics2D.SizeFromAssets("Assets/UI/EntityHealthBar.png").As1920x1080() * .4f;
                _backgroundTextureSize =
                    Graphics2D.SizeFromAssets("Assets/UI/EntityHealthBarBackground.png").As1920x1080() * .4f;
                _backgroundTextureId = Graphics2D.LoadFromAssets("Assets/UI/EntityHealthBarBackground.png");
                //TextureRegistry.MarkStatic(_backgroundTextureId);
            });
        }

        public HealthBarComponent(IEntity Parent, string Name, HealthBarType Type) : this(Parent, Name, Type,
            ColorFromType(Type))
        {
        }

        public HealthBarComponent(IEntity Parent, string Name, HealthBarType Type, Color FontColor) : base(Parent)
        {
            _healthBar = new TexturedBar(
                TextureFromType(Type),
                Vector2.Zero,
                Vector2.One,
                () => Parent.Health,
                () => Parent.MaxHealth
            );
            _backgroundTexture = new RenderableTexture(
                new BackgroundTexture(
                    0,
                    _healthBar.Position,
                    _healthBar.Scale
                ),
                DrawOrder.After
            );
            Executer.ExecuteOnMainThread(
                () =>
                {
                    _healthBar.TextureId = TextureFromType(Type);
                    _backgroundTexture.BaseTexture.TextureElement.TextureId = _backgroundTextureId;
                });
            _text = new RenderableText(
                string.Empty,
                Vector2.Zero, Type == HealthBarType.Hostile || Type == HealthBarType.Black || Type == HealthBarType.Gold
                    ? FontColor
                    : Type == HealthBarType.Neutral
                        ? Color.White
                        : Friendly,
                FontCache.GetBold(11)
            );
            _text.Stroke = true;
            this.Name = Name;
            DrawManager.UIRenderer.Remove(_text);
            DrawManager.UIRenderer.Remove(_backgroundTexture);
            DrawManager.UIRenderer.Remove(_healthBar);
            DrawManager.UIRenderer.Add(this, DrawOrder.After);
            _panel = new Panel();
            _panel.AddElement(_text);
            _panel.AddElement(_healthBar);
            _panel.AddElement(_backgroundTexture);
            _panel.OnPanelStateChange += OnPanelStateChanged;
            GameManager.Player.UI.GamePanel.AddElement(_panel);
        }

        public bool Hide { get; set; }

        public float? Height { get; set; }

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

        public override void Draw()
        {
            if (Parent.Model == null || _barSize < 0.05f) return;

            var height = Height ?? Math.Max(Parent.Model.Height * 1.5f, 6);
            var eyeSpace = Vector4.Transform(
                new Vector4(Parent.Position + height * Vector3.UnitY, 1),
                Culling.ModelViewMatrix
            );
            var homogeneousSpace = Vector4.Transform(eyeSpace, Culling.ProjectionMatrix);
            var ndc = homogeneousSpace.Xyz() / homogeneousSpace.W;
            _healthBar.Position = Mathf.Clamp(ndc.Xy(), -.98f, .98f);
            _healthBar.Scale = _textureSize * _barSize;
            _backgroundTexture.Position = _healthBar.Position;
            _backgroundTexture.Scale = _backgroundTextureSize;
            _text.Position = _healthBar.Position + _healthBar.Scale.Y * Vector2.UnitY * 3;

            _healthBar.Draw();
            _backgroundTexture.Draw();
            _text.Draw();
        }

        public override void Dispose()
        {
            DrawManager.UIRenderer.Remove(this);
            GameManager.Player.UI.GamePanel.RemoveElement(_panel);
            _panel.RemoveElement(_text);
            _panel.RemoveElement(_healthBar);
            _panel.OnPanelStateChange -= OnPanelStateChanged;
            _backgroundTexture.Dispose();
            _text.Dispose();
            _healthBar.Dispose();
            _panel.Dispose();
        }

        public override void Update()
        {
            _show = (Parent.Position - GameManager.Player.Position).LengthSquared() <
                    ShowDistance * ShowDistance
                    && !Hide
                    && !Parent.IsDead
                    && !GameSettings.Paused
                    && !GameManager.IsLoading
                    && !Parent.Physics.StaticRaycast(GameManager.Player.Position +
                                                     Vector3.UnitY * GameManager.Player.Model.Height * .5f);

            _targetBarSize = _show ? 1 : 0;

            _barSize = Mathf.Lerp(_barSize, _targetBarSize, Time.DeltaTime * 16f);
            _text.Scale = _originalTextScale * _barSize * _textEnabled;

            var product = Vector3.Dot(GameManager.Player.View.CrossDirection,
                (Parent.Position - GameManager.Player.Position).NormalizedFast());
            _lastProduct = Mathf.Lerp(_lastProduct, product, Time.DeltaTime * 2f);
            if (_barSize <= 0.5f || _lastProduct <= 0f)
            {
                _healthBar.Disable();
                _text.Disable();
                _backgroundTexture.Disable();
                _targetBarSize = 0;
                _textEnabled = 0;
            }
            else
            {
                _healthBar.Enable();
                _text.Enable();
                _backgroundTexture.Enable();
                _targetBarSize = 1;
                _textEnabled = 1;
            }
        }

        private void OnPanelStateChanged(object Sender, PanelState State)
        {
            if (!_show) _text.Disable();
        }

        private static Color ColorFromType(HealthBarType Type)
        {
            return Type == HealthBarType.Neutral
                ? Friendly
                : Type == HealthBarType.Hostile
                    ? Hostile
                    : Type == HealthBarType.Friendly
                        ? Friendly
                        : Neutral;
        }

        private static uint TextureFromType(HealthBarType Type)
        {
            if (Type == HealthBarType.Black) return _blackTexture;
            if (Type == HealthBarType.Gold) return _goldTexture;
            return Type == HealthBarType.Neutral
                ? _neutralTexture
                : Type == HealthBarType.Hostile
                    ? _hostileTexture
                    : Type == HealthBarType.Friendly
                        ? _friendlyTexture
                        : throw new ArgumentOutOfRangeException($"Texture for type '{Type}' doesn't exists.");
        }
    }

    public enum HealthBarType
    {
        Hostile,
        Friendly,
        Neutral,
        Black,
        Gold
    }
}