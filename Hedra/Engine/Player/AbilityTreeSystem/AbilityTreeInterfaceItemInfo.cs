using System;
using System.Drawing;
using Hedra.Core;
using Hedra.Engine.Localization;
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
            ItemDescription.Text = BuildItemDescription(realSkill);
            ItemDescription.Color = Color.White;
            ItemText.Text = Utils.FitString(realSkill.DisplayName, 15);

            ItemTexture.TextureElement.TextureId = CurrentItem.HasAttribute("ImageId") 
                ? CurrentItem.GetAttribute<uint>("ImageId") 
                : GUIRenderer.TransparentTexture;
            SetPosition();
        }

        private static string BuildItemDescription(BaseSkill RealSkill)
        {
            var manaCost = RealSkill.ManaCost > 0
                ? $"{Translations.Get("skill_mana_cost", RealSkill.ManaCost)}{Environment.NewLine}"
                : string.Empty;
            var cooldown = RealSkill.MaxCooldown > 0 
                ? Translations.Get("skill_cooldown", RealSkill.MaxCooldown) 
                : string.Empty;
            return $"{Utils.FitString(RealSkill.Description, 25)}{Environment.NewLine}{manaCost}{cooldown}";
        }

        protected virtual void SetPosition()
        {
            ItemDescription.Position = this.Position - Mathf.ScaleGui(_targetResolution, Vector2.UnitY * .2f);
            ItemTexture.Position = this.Position + Mathf.ScaleGui(_targetResolution, Vector2.UnitY * .05f);
            SetTitlePosition();
        }
    }
}
