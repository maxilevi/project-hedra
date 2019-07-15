using System;
using Hedra.Engine.Player;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Rendering.UI
{
    public class ProfileUIElement : Panel
    {
        public readonly RenderableTexture ClassLogo;
        private readonly BackgroundTexture _healthBackground;
        private readonly BackgroundTexture _xpBackground;
        protected readonly BackgroundTexture _manaBackground;
        private readonly TexturedBar _healthBar;
        protected readonly TexturedBar _manaBar;
        private readonly TexturedBar _xpBar;
        private readonly Vector2 _healthOffset;
        private readonly Vector2 _xpOffset;
        private readonly Vector2 _manaOffset;
        private readonly float _scale;
        private readonly IPlayer _player;

        public ProfileUIElement(IPlayer Player, Vector2 Position, float Scale)
        {
            _player = Player;
            _scale = Scale;
            _healthOffset = new Vector2(0, .015f) / 0.75f;
            _xpOffset = new Vector2(0, .055f) / 0.75f;
            _manaOffset = new Vector2(0, -0.0345f) / 0.75f;
            _healthBackground = new BackgroundTexture(
                Graphics2D.LoadFromAssets("Assets/UI/HealthBarBackground.png"), Vector2.Zero, Graphics2D.SizeFromAssets("Assets/UI/HealthBarBackground.png").As1920x1080() * Scale
            );
            _manaBackground = new BackgroundTexture(
                Graphics2D.LoadFromAssets("Assets/UI/ManaBarBackground.png"), Vector2.Zero, Graphics2D.SizeFromAssets("Assets/UI/ManaBarBackground.png").As1920x1080()  * Scale
            );
            _xpBackground = new BackgroundTexture(
                Graphics2D.LoadFromAssets("Assets/UI/XpBarBackground.png"), Vector2.Zero, Graphics2D.SizeFromAssets("Assets/UI/XpBarBackground.png").As1920x1080()  * Scale
            );

            _healthBar = new TexturedBar(
                Graphics2D.LoadFromAssets("Assets/UI/HealthBar.png"),
                Vector2.Zero,
                Graphics2D.SizeFromAssets("Assets/UI/HealthBar.png").As1920x1080()  * Scale,
                () => Health,
                () => MaxHealth
            );

            _manaBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/ManaBar.png"), Vector2.Zero, Graphics2D.SizeFromAssets("Assets/UI/ManaBar.png").As1920x1080()  * Scale,
                () => Player.Mana,
                () => Player.MaxMana);

            _xpBar = new TexturedBar(Graphics2D.LoadFromAssets("Assets/UI/XpBar.png"), Vector2.Zero, Graphics2D.SizeFromAssets("Assets/UI/XpBar.png").As1920x1080() * Scale,
                () => XP,
                () => MaxXP);
            
            ClassLogo = new RenderableTexture(new BackgroundTexture(0, Position, Vector2.One), DrawOrder.After);
            
            AddElement(_healthBackground);
            AddElement(_manaBackground);
            AddElement(_xpBackground);
            AddElement(_xpBar);
            AddElement(ClassLogo);
            AddElement(_healthBar);
            AddElement(_manaBar);
        }

        public void Update()
        {
            var position = ClassLogo.Position;
            _healthBackground.Position = new Vector2(position.X + _healthBackground.Scale.X * 2f * _scale, position.Y + _healthOffset.Y * _scale);
            _manaBackground.Position = new Vector2(position.X + _manaBackground.Scale.X * 2f * _scale, position.Y + _manaOffset.Y * _scale);
            _xpBackground.Position = new Vector2(position.X + _xpBackground.Scale.X * 2f * _scale, position.Y + _xpOffset.Y * _scale);
            _healthBar.Position = new Vector2(position.X + _healthBar.Scale.X * 2f * _scale, _healthBackground.Position.Y);
            _manaBar.Position = new Vector2(position.X + _manaBar.Scale.X * 2f * _scale, _manaBackground.Position.Y);
            _xpBar.Position = new Vector2(position.X + _xpBar.Scale.X * 2f * _scale, _xpBackground.Position.Y);
        }

        protected virtual float MaxHealth => _player.MaxHealth;
        protected virtual float Health => _player.Health;
        protected virtual float MaxXP => _player.MaxXP;
        protected virtual float XP => _player.XP;
    }
}