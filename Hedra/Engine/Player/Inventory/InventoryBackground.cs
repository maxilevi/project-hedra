using System.Drawing;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player.Inventory
{
    public class InventoryBackground
    {
        private readonly GUIText _name;
        private readonly GUIText _level;
        private readonly GUIText _health;
        private readonly GUIText _mana;
        private readonly GUIText _xp;
        private readonly GUIText _gold;
        private readonly Texture _texture;
        private readonly Panel _panel;
        private bool _enabled;

        public InventoryBackground(Vector2 Position)
        {
            _texture = new Texture("Assets/UI/InventoryBackground.png", Vector2.Zero, Vector2.One);
            _name = new GUIText(string.Empty, Position + Vector2.UnitY * .075f,
                Color.White, FontCache.Get(AssetManager.Fonts.Families[0], 24, FontStyle.Bold));
            _level = new GUIText(string.Empty, Position + Vector2.UnitY * -.05f,
                Color.White, FontCache.Get(UserInterface.Fonts.Families[0], 16));
            _health = new GUIText(string.Empty, Position + Vector2.UnitX * -.2f + Vector2.UnitY * .075f + Vector2.UnitY * -.05f,
                Color.Red, FontCache.Get(AssetManager.Fonts.Families[0], 14, FontStyle.Bold));
            _mana = new GUIText(string.Empty, Position + Vector2.UnitX * -.2f + Vector2.UnitY * -.025f + Vector2.UnitY * -.05f,
                Color.DodgerBlue, FontCache.Get(UserInterface.Fonts.Families[0], 10));
            _xp = new GUIText(string.Empty, Position + Vector2.UnitX * .2f + Vector2.UnitY * .075f + Vector2.UnitY * -.05f,
                Color.DarkViolet, FontCache.Get(AssetManager.Fonts.Families[0], 14, FontStyle.Bold));
            _gold = new GUIText(string.Empty, Position + Vector2.UnitX * .2f + Vector2.UnitY * -.025f + Vector2.UnitY * -.05f,
                Color.Gold, FontCache.Get(UserInterface.Fonts.Families[0], 10));

            _panel = new Panel();
            _panel.AddElement(_texture);
            _panel.AddElement(_name);
            _panel.AddElement(_level);
            _panel.AddElement(_health);
            _panel.AddElement(_mana);
            _panel.AddElement(_xp);
            _panel.AddElement(_gold);
            this.Position = Position;
            this.Scale = Scale;
        }

        public void UpdateView(Humanoid Human)
        {
            _name.Text = Human.Name;
            _level.Text = "LEVEL "+Human.Level;
            _health.Text = $"{(int) Human.Health} HP";
            _mana.Text = $"{(int)Human.Mana} MP";
            _xp.Text = $"{(int)Human.XP}/{(int)Human.MaxXP} XP";
            _gold.Text = "256 G";
        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                if (_enabled)
                    _panel.Enable();
                else
                    _panel.Disable();
            }
        }

        public Vector2 Scale
        {
            get { return _texture.Scale; }
            set { _texture.Scale = value; }
        }

        public Vector2 Position
        {
            get { return _texture.Position; }
            set { _texture.Position = value; }
        }
    }
}
