using System.Drawing;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player.Inventory
{
    public class InventoryBackground
    {
        protected readonly GUIText Name;
        protected readonly GUIText Level;
        protected readonly GUIText TopLeftText;
        protected readonly GUIText BottomLeftText;
        protected readonly GUIText TopRightText;
        protected readonly GUIText BottomRightText;
        private readonly Vector2 _targetResolution;
        private readonly Texture _texture;
        private readonly Panel _panel;
        private bool _enabled;

        public InventoryBackground(Vector2 Position)
        {
            _targetResolution = new Vector2(1366, 768);
            _texture = new Texture("Assets/UI/InventoryBackground.png", Vector2.Zero, Mathf.ScaleGUI(_targetResolution, Vector2.One * .55f));
            Name = new GUIText(string.Empty, Position + Vector2.UnitY * .075f,
                Color.White, FontCache.Get(AssetManager.BoldFamily, 24, FontStyle.Bold));
            Level = new GUIText(string.Empty, Position + Vector2.UnitY * -.05f,
                Color.White, FontCache.Get(UserInterface.Fonts.Families[0], 16));
            TopLeftText = new GUIText(string.Empty, Position + Vector2.UnitX * -.2f + Vector2.UnitY * .075f + Vector2.UnitY * -.05f,
                Color.Red, FontCache.Get(AssetManager.BoldFamily, 14, FontStyle.Bold));
            BottomLeftText = new GUIText(string.Empty, Position + Vector2.UnitX * -.2f + Vector2.UnitY * -.025f + Vector2.UnitY * -.05f,
                Color.DodgerBlue, FontCache.Get(UserInterface.Fonts.Families[0], 10));
            TopRightText = new GUIText(string.Empty, Position + Vector2.UnitX * .2f + Vector2.UnitY * .075f + Vector2.UnitY * -.05f,
                Color.DarkViolet, FontCache.Get(AssetManager.BoldFamily, 14, FontStyle.Bold));
            BottomRightText = new GUIText(string.Empty, Position + Vector2.UnitX * .2f + Vector2.UnitY * -.025f + Vector2.UnitY * -.05f,
                Color.Gold, FontCache.Get(UserInterface.Fonts.Families[0], 10));

            _panel = new Panel();
            _panel.AddElement(_texture);
            _panel.AddElement(Name);
            _panel.AddElement(Level);
            _panel.AddElement(TopLeftText);
            _panel.AddElement(BottomLeftText);
            _panel.AddElement(TopRightText);
            _panel.AddElement(BottomRightText);
            this.Position = Position;
        }

        public virtual void UpdateView(Humanoid Human)
        {
            Name.Text = Human.Name;
            Level.Text = "LEVEL "+Human.Level;
            TopLeftText.Text = $"{(int) Human.Health} HP";
            BottomLeftText.Text = $"{(int)Human.Mana} MP";
            TopRightText.Text = $"{(int)Human.XP}/{(int)Human.MaxXP} XP";
            var gold = Human.Gold == int.MaxValue ? "∞" : Human.Gold.ToString();
            BottomRightText.Text = $"{gold} G";
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

        public Vector2 Position
        {
            get { return _texture.Position; }
            set { _texture.Position = value; }
        }
    }
}
