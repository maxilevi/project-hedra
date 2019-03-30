using System.Drawing;
using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.UI;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.Player.Inventory
{
    public class InventoryBackground : BaseBackground
    {
        public static readonly uint DefaultId = Graphics2D.LoadFromAssets("Assets/UI/InventoryBackground.png");
        public static readonly Vector2 DefaultSize = Graphics2D.SizeFromAssets("Assets/UI/InventoryBackground.png").As1920x1080() * InventoryArrayInterface.UISizeMultiplier;
        
        protected GUIText Name { get; }
        protected GUIText Level { get; }
        protected GUIText TopLeftText { get;  }
        protected GUIText BottomLeftText { get; }
        protected GUIText TopRightText { get; }
        protected GUIText BottomRightText { get; }
        
        public InventoryBackground(Vector2 Position) : base(Position, Vector2.One * .55f)
        {
            var offsetX = Vector2.UnitX * DefaultSize * .35f;
            var offsetY = Vector2.UnitY * DefaultSize * .25f;
            Name = new GUIText(string.Empty, Position + offsetY,
                Color.White, FontCache.GetBold(24));
            Level = new GUIText(string.Empty, Position - offsetY,
                Color.White, FontCache.GetNormal(16));
            TopLeftText = new GUIText(string.Empty, Position + offsetY - offsetX,
                Color.Red, FontCache.GetBold(14));
            BottomLeftText = new GUIText(string.Empty, Position - offsetY - offsetX,
                Color.DodgerBlue, FontCache.GetNormal(10));
            TopRightText = new GUIText(string.Empty, Position + offsetY + offsetX,
                Color.DarkViolet, FontCache.GetBold(14));
            BottomRightText = new GUIText(string.Empty, Position - offsetY + offsetX,
                Color.Gold, FontCache.GetNormal(10));

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
