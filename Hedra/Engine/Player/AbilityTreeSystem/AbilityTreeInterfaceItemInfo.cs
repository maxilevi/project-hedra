using System;
using System.Drawing;
using Hedra.Core;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.SkillSystem;
using OpenTK;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    public class AbilityTreeInterfaceItemInfo : InventoryInterfaceItemInfo
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
            ItemDescription.Position = this.Position - Mathf.ScaleGui(_targetResolution, Vector2.UnitY * .2f);
            ItemText.Text = Utils.FitString(realSkill.DisplayName, 15);

            ItemTexture.Position = this.Position + Mathf.ScaleGui(_targetResolution, Vector2.UnitY * .05f);
            ItemTexture.TextureElement.TextureId = CurrentItem.HasAttribute("ImageId") 
                ? CurrentItem.GetAttribute<uint>("ImageId") 
                : GUIRenderer.TransparentTexture;
            SetTitlePosition();
        }
    }
}
