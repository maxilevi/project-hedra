using System.Drawing;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering.UI;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.Player.Inventory
{
    public class InventoryBackground : BaseBackground
    {
        protected GUIText Name { get; private set; }
        protected GUIText Level { get; private set; }
        protected GUIText TopLeftText { get; private set; }
        protected GUIText BottomLeftText { get; private set; }
        protected GUIText TopRightText { get; private set; }
        protected GUIText BottomRightText { get; private set; }
        private Vector2 _targetResolution;
        
        public InventoryBackground(Vector2 Position) : base(Position, Vector2.One * .55f)
        {
            _targetResolution = new Vector2(1366, 705);
            Name = new GUIText(string.Empty, Position + Mathf.ScaleGui(_targetResolution, Vector2.UnitY * .075f),
                Color.White, FontCache.Get(AssetManager.BoldFamily, 24, FontStyle.Bold));
            Level = new GUIText(string.Empty, Position + Mathf.ScaleGui(_targetResolution, Vector2.UnitY * -.05f),
                Color.White, FontCache.Get(AssetManager.NormalFamily, 16));
            TopLeftText = new GUIText(string.Empty, Position + Mathf.ScaleGui(_targetResolution, Vector2.UnitX * -.2f + Vector2.UnitY * .075f + Vector2.UnitY * -.05f),
                Color.Red, FontCache.Get(AssetManager.BoldFamily, 14, FontStyle.Bold));
            BottomLeftText = new GUIText(string.Empty, Position + Mathf.ScaleGui(_targetResolution, Vector2.UnitX * -.2f + Vector2.UnitY * -.025f + Vector2.UnitY * -.05f),
                Color.DodgerBlue, FontCache.Get(AssetManager.NormalFamily, 10));
            TopRightText = new GUIText(string.Empty, Position + Mathf.ScaleGui(_targetResolution, Vector2.UnitX * .2f + Vector2.UnitY * .075f + Vector2.UnitY * -.05f),
                Color.DarkViolet, FontCache.Get(AssetManager.BoldFamily, 14, FontStyle.Bold));
            BottomRightText = new GUIText(string.Empty, Position + Mathf.ScaleGui(_targetResolution, Vector2.UnitX * .2f + Vector2.UnitY * -.025f + Vector2.UnitY * -.05f),
                Color.Gold, FontCache.Get(AssetManager.NormalFamily, 10));

            Panel.AddElement(Name);
            Panel.AddElement(Level);
            Panel.AddElement(TopLeftText);
            Panel.AddElement(BottomLeftText);
            Panel.AddElement(TopRightText);
            Panel.AddElement(BottomRightText);
        }

        public virtual void UpdateView(IHumanoid Human)
        {
            Name.Text = Human.Name;
            Level.Text = $"LEVEL {Human.Level}";
            TopLeftText.Text = $"{(int) Human.Health}/{(int) Human.MaxHealth} HP";
            BottomLeftText.Text = $"{(int)Human.Mana}/{(int) Human.MaxMana} MP";
            TopRightText.Text = $"{(int)Human.XP}/{(int)Human.MaxXP} XP";
            var gold = Human.Gold == int.MaxValue ? "∞" : Human.Gold.ToString();
            BottomRightText.Text = $"{gold} G";
        }
    }
}
