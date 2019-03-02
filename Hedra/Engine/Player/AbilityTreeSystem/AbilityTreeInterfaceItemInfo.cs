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
            BackgroundTexture.Scale *= 1.125f;
            ItemTexture.Scale *= .4f;
            ItemTexture.TextureElement.MaskId = InventoryArrayInterface.DefaultId;
        }

        protected override void UpdateView()
        {
            var realSkill = CurrentItem.GetAttribute<BaseSkill>("Skill");
            var originalLevel = realSkill.Level;
            try
            {
                realSkill.Level = Math.Max(1, originalLevel);
                ItemDescription.Text = TextProvider.Wrap(realSkill.Description, 35);
                ItemDescription.Color = Color.White;
                ItemText.Text = Utils.FitString(realSkill.DisplayName, 18);
                ItemAttributes.Text = TextProvider.Wrap(BuildAttributes(realSkill), 30);
                ItemTexture.TextureElement.TextureId = CurrentItem.HasAttribute("ImageId")
                    ? CurrentItem.GetAttribute<uint>("ImageId")
                    : GUIRenderer.TransparentTexture;
                SetPosition();
            }
            finally
            {
                realSkill.Level = originalLevel;
            }
        }

        private static string BuildAttributes(BaseSkill RealSkill)
        {
            var manaCost = RealSkill.ManaCost > 0
                ? $"• {Translations.Get("skill_mana_cost", RealSkill.ManaCost.ToString("0.0", CultureInfo.InvariantCulture))}{Environment.NewLine}"
                : string.Empty;
            var cooldown = RealSkill.MaxCooldown > 0
                ? $"• {Translations.Get("skill_cooldown", RealSkill.MaxCooldown.ToString("0.0", CultureInfo.InvariantCulture))}{Environment.NewLine}"
                : string.Empty;
            var passive = RealSkill.Passive
                ? $"{Translations.Get("passive_skill")}{Environment.NewLine}"
                : string.Empty;
            var attributes = new StringBuilder();
            var skillAttributes = RealSkill.Attributes;
            for (var i = 0; i < skillAttributes.Length; ++i)
            {
                attributes.AppendLine($"• {skillAttributes[i]}");
            }
            return $"{passive}{manaCost}{cooldown}{attributes}";
        }
        
        protected virtual void SetPosition()
        {
            ItemDescription.Position = this.Position - Mathf.ScaleGui(_targetResolution, Vector2.UnitY * .0f);
            ItemTexture.Position = this.Position + Mathf.ScaleGui(_targetResolution, Vector2.UnitY * .2f);
            ItemAttributes.Position = ItemDescription.Position - (ItemDescription.Scale.Y + ItemAttributes.Scale.Y) * Vector2.UnitY - Mathf.ScaleGui(_targetResolution, Vector2.UnitY * .075f);
            SetTitlePosition();
        }
    }
}
