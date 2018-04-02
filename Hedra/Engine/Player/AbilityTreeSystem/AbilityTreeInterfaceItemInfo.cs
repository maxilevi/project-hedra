using System.Drawing;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    public class AbilityTreeInterfaceItemInfo : InventoryInterfaceItemInfo
    {
        public AbilityTreeInterfaceItemInfo(InventoryItemRenderer Renderer) : base(Renderer)
        {
            ItemTexture.Scale *= .4f;
            ItemTexture.TextureElement.MaskId = InventoryArrayInterface.DefaultId;
        }

        protected override void UpdateView()
        {
            ItemDescription.Color = Color.White;
            ItemDescription.Text = CurrentItem.Description;
            ItemDescription.Position = this.Position - Vector2.UnitY * .15f;
            ItemText.Text = Utils.FitString(CurrentItem.DisplayName.ToUpperInvariant(), 15);

            ItemTexture.Position = Vector2.UnitY * .05f + this.Position;
            ItemTexture.TextureElement.TextureId = CurrentItem.HasAttribute("ImageId") 
                ? CurrentItem.GetAttribute<uint>("ImageId") 
                : GUIRenderer.TransparentTexture;
        }
    }
}
