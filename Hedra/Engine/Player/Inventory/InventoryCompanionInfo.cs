using System.Drawing;
using System.Text;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Scripting;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Rendering;
using Hedra.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player.Inventory
{
    public sealed class InventoryCompanionInfo : InventoryInterfaceItemInfo
    {
        private static readonly Script Script = Interpreter.GetScript("Companion.py");
        private GUIText Level { get; }
        private GUIText TopLeftText { get;  }
        private GUIText BottomLeftText { get; }
        private GUIText TopRightText { get; }
        private GUIText BottomRightText { get; }
        private IEntity _companion;
        
        public InventoryCompanionInfo() : base((1f / (1366f / GameSettings.Width)) * .9f)
        {
            Level = new GUIText(string.Empty, Vector2.Zero, Color.White, FontCache.GetNormal(16));
            TopLeftText = new GUIText(string.Empty, Vector2.Zero, Color.Red, FontCache.GetBold(14));
            BottomLeftText = new GUIText(string.Empty, Vector2.Zero, Color.Yellow, FontCache.GetNormal(10));
            TopRightText = new GUIText(string.Empty, Vector2.Zero, Color.DarkViolet, FontCache.GetBold(14));
            BottomRightText = new GUIText(string.Empty, Vector2.Zero, Color.LightBlue, FontCache.GetNormal(10));
            ItemText.TextFont = FontCache.GetBold(28);
            
            Panel.AddElement(Level);
            Panel.AddElement(TopLeftText);
            Panel.AddElement(BottomLeftText);
            Panel.AddElement(TopRightText);
            Panel.AddElement(BottomRightText);
        }

        public void Show(Item Item, IEntity Companion)
        {
            _companion = Companion;
            base.Show(Item);
        }

        public override void Hide()
        {
            _companion = null;
            base.Hide();
        }

        protected override void AddLayout()
        {
            base.AddLayout();
            var marginX = Vector2.UnitX * BackgroundTexture.Scale * .5f;
            var marginY = BackgroundTexture.Scale * .095f * Vector2.UnitY;
            var offset = -BackgroundTexture.Scale * .7f * Vector2.UnitY;
            var statsOffset = -BackgroundTexture.Scale * .05f * Vector2.UnitY;
            
            Level.Position = Position + marginY * 3f + offset + BackgroundTexture.Scale * .05f * Vector2.UnitY;
            TopLeftText.Position = Position + marginY - marginX + offset + statsOffset;
            BottomLeftText.Position = Position - marginY - marginX + offset + statsOffset;
            TopRightText.Position = Position + marginY + marginX + offset + statsOffset;
            BottomRightText.Position = Position - marginY + marginX + offset + statsOffset;

            ItemTexture.Scale *= .65f;
            ItemTexture.Position += BackgroundTexture.Scale * Vector2.UnitY * .15f;
            Script.Execute("setup_ui", CurrentItem, _companion, TopLeftText, TopRightText, BottomLeftText, BottomRightText, Level, ItemText);
        }

        protected override void UpdateItemMesh()
        {
            UpdateItemMesh(CurrentItem.GetAttribute<VertexData>("CompanionModel"));
        }
        
        protected override float DescriptionHeight => BackgroundTexture.Scale.Y * .1f;

        protected override void AddAttributes()
        {
            ItemAttributes.Text = string.Empty;
        }

        public Item ShowingCompanion => CurrentItem;
    }
}