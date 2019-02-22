using System;
using System.Drawing;
using System.Globalization;
using System.Text;
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
            BackgroundTexture.Scale *= 1.1f;
            ItemTexture.Scale *= .4f;
            ItemTexture.TextureElement.MaskId = InventoryArrayInterface.DefaultId;
        }

        protected override void UpdateView()
        {
            var realSkill = CurrentItem.GetAttribute<BaseSkill>("Skill");
            ItemDescription.Text = BuildItemDescription(realSkill);
            ItemDescription.Color = Color.White;
            ItemText.Text = Utils.FitString(realSkill.DisplayName, 18);

            ItemTexture.TextureElement.TextureId = CurrentItem.HasAttribute("ImageId") 
                ? CurrentItem.GetAttribute<uint>("ImageId")
                : GUIRenderer.TransparentTexture;
            SetPosition();
        }

        private static string BuildItemDescription(BaseSkill RealSkill)
        {
            return $"{TextProvider.Wrap(RealSkill.Description, 25)}{Environment.NewLine}{Environment.NewLine}{BuildAttributes(RealSkill)}";
        }

        private static string BuildAttributes(BaseSkill RealSkill)
        {
            var manaCost = RealSkill.ManaCost > 0
                ? $"• {Translations.Get("skill_mana_cost", RealSkill.ManaCost.ToString("0.0", CultureInfo.InvariantCulture))}{Environment.NewLine}"
                : string.Empty;
            var cooldown = RealSkill.MaxCooldown > 0
                ? $"• {Translations.Get("skill_cooldown", RealSkill.MaxCooldown.ToString("0.0", CultureInfo.InvariantCulture))}{Environment.NewLine}"
                : string.Empty;
            var attributes = new StringBuilder();
            var skillAttributes = RealSkill.Attributes;
            for (var i = 0; i < skillAttributes.Length; ++i)
            {
                attributes.AppendLine($"• {skillAttributes[i]}");
            }
            return $"{manaCost}{cooldown}{attributes}";
        }
        
        protected virtual void SetPosition()
        {
            ItemDescription.Position = this.Position - Mathf.ScaleGui(_targetResolution, Vector2.UnitY * .15f);
            ItemTexture.Position = this.Position + Mathf.ScaleGui(_targetResolution, Vector2.UnitY * .15f);
            SetTitlePosition();
        }
    }
}
