using System;
using System.Drawing;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Player.Skills;
using Hedra.Engine.Rendering.UI;
using OpenTK;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    internal class AbilityTreeInterfaceItemInfo : InventoryInterfaceItemInfo
    {
        private readonly Vector2 _targetResolution = new Vector2(1366, 705);
        public AbilityTreeInterfaceItemInfo(InventoryItemRenderer Renderer) : base(Renderer)
        {
            ItemTexture.Scale *= .4f;
            ItemTexture.TextureElement.MaskId = InventoryArrayInterface.DefaultId;
        }

        protected override void UpdateView()
        {
            var realSkill = CurrentItem.GetAttribute<BaseSkill>("Skill");
            ItemDescription.Text = $"{Utils.FitString(realSkill.Description, 25)}" +
                              (realSkill.ManaCost != 0 ? $"Mana cost : {realSkill.ManaCost}{Environment.NewLine}" : string.Empty) +
                              (realSkill.MaxCooldown != 0 ? $"Cooldown : {realSkill.MaxCooldown}" : string.Empty);
            ItemDescription.Color = Color.White;
            ItemDescription.Position = this.Position - Mathf.ScaleGUI(_targetResolution, Vector2.UnitY * .15f);
            ItemText.Text = Utils.FitString(CurrentItem.DisplayName.ToUpperInvariant(), 15);

            ItemTexture.Position = this.Position + Mathf.ScaleGUI(_targetResolution, Vector2.UnitY * .05f);
            ItemTexture.TextureElement.TextureId = CurrentItem.HasAttribute("ImageId") 
                ? CurrentItem.GetAttribute<uint>("ImageId") 
                : GUIRenderer.TransparentTexture;
        }
    }
}
