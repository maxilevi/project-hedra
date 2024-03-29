/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/07/2016
 * Time: 12:20 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Numerics;
using Hedra.Components;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.Windowing;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Numerics;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using SixLabors.ImageSharp;

namespace Hedra.Engine.EntitySystem.BossSystem
{
    /// <inheritdoc />
    /// <summary>
    ///     Description of BossHealthBarComponent.
    /// </summary>
    public class BossHealthBarComponent : BaseHealthBarComponent, INamedHealthBar
    {
        private static uint _bossBarTexture;
        private static readonly Vector2 _bossBarTextureSize;
        private static readonly Vector2 _backgroundTextureSize;
        private static uint _backgroundTextureId;
        private readonly BackgroundTexture _backgroundTexture;
        private readonly Vector2 _barDefaultPosition;
        private readonly TexturedBar _healthBar;
        private readonly GUIText _nameText;
        private readonly Vector2[] _originalScales;

        private readonly Panel _panel;
        private readonly GUIText _percentageText;
        private bool _initialized;
        private string _name;
        private float _targetSize;

        static BossHealthBarComponent()
        {
            Executer.ExecuteOnMainThread(() =>
            {
                _bossBarTexture = Graphics2D.LoadTexture(new BitmapObject
                    {
                        Bitmap = Graphics2D.LoadBitmapFromAssets("Assets/UI/BossHealthBar.png"),
                        Path = "UI:Color:BossHealthBarComponent:BossHostile"
                    }, TextureMinFilter.Nearest, TextureMagFilter.Nearest, TextureWrapMode.ClampToEdge
                );
                _backgroundTextureId = Graphics2D.LoadFromAssets("Assets/UI/BossHealthBarBackground.png");
            });
            _bossBarTextureSize = Graphics2D.SizeFromAssets("Assets/UI/BossHealthBar.png").As1920x1080() * .65f;
            _backgroundTextureSize =
                Graphics2D.SizeFromAssets("Assets/UI/BossHealthBarBackground.png").As1920x1080() * .65f;
        }

        public BossHealthBarComponent(IEntity Parent, string Name) : base(Parent)
        {
            this.Name = Name;
            _panel = new Panel();
            _backgroundTexture = new BackgroundTexture(
                0,
                _barDefaultPosition = new Vector2(0f, .75f),
                _backgroundTextureSize
            );
            _healthBar = new TexturedBar(
                0,
                _backgroundTexture.Position,
                _bossBarTextureSize,
                () => Parent.Health,
                () => Parent.MaxHealth,
                DrawOrder.Before
            );
            _percentageText = new GUIText(string.Empty, _healthBar.Position, Color.White, FontCache.GetBold(10));
            _nameText = new GUIText(Name, new Vector2(0f, .815f), Color.White, FontCache.GetBold(12));

            _panel.AddElement(_backgroundTexture);
            _panel.AddElement(_nameText);
            _panel.AddElement(_healthBar);
            _panel.AddElement(_percentageText);

            var elements = _panel.Elements;
            _originalScales = new Vector2[elements.Count];
            for (var i = 0; i < elements.Count; ++i)
            {
                _originalScales[i] = elements[i].Scale;
                elements[i].Scale = Vector2.Zero;
            }

            _panel.Disable();

            GameManager.Player.UI.GamePanel.AddElement(_panel);
            Executer.ExecuteOnMainThread(
                () =>
                {
                    _healthBar.TextureId = _bossBarTexture;
                    _backgroundTexture.TextureElement.TextureId = _backgroundTextureId;
                }
            );
        }

        public bool Enabled { get; set; } = true;
        public int ViewRange { get; set; } = 128;

        private bool CanShow => Enabled && GameManager.Player.UI.GamePanel.Enabled &&
                                (Parent.Position - GameManager.Player.Position).LengthSquared() < ViewRange * ViewRange;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                if (_nameText != null)
                {
                    _nameText.Text = _name;
                    _originalScales[_panel.Elements.IndexOf(_nameText)] = _nameText.Scale;
                }
            }
        }

        public override void Update()
        {
            _targetSize = CanShow ? 1 : 0;
            var elements = _panel.Elements;
            for (var i = 0; i < elements.Count; ++i)
            {
                if (_percentageText == elements[i])
                {
                    if (_targetSize < 1)
                        _percentageText.Disable();
                    else
                        _percentageText.Enable();
                }
                else
                {
                    elements[i].Scale = Mathf.Lerp(elements[i].Scale, _originalScales[i] * _targetSize,
                        Time.IndependentDeltaTime * 8f);
                    DisableIfSmall(elements[i]);
                }

                if (!GameManager.Player.UI.GamePanel.Enabled)
                    elements[i].Disable();
            }

            _percentageText.Text = $"{(int)Parent.Health}/{(int)Parent.MaxHealth}";
        }

        private static void DisableIfSmall(UIElement Element)
        {
            if (Element.Scale.LengthSquared() < 0.001 * 0.001)
                Element.Disable();
            else
                Element.Enable();
        }

        public override void Dispose()
        {
            GameManager.Player.UI.GamePanel.RemoveElement(_panel);
            var elements = _panel.Elements;
            for (var i = 0; i < elements.Count; ++i)
                elements[i].Dispose();
        }
    }
}