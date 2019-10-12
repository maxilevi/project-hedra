using System;
using System.Drawing;
using System.Globalization;
using System.Text;
using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Player.Inventory;
using Hedra.Engine.Rendering.UI;
using Hedra.Engine.SkillSystem;
using Hedra.Localization;
using OpenToolkit.Mathematics;

namespace Hedra.Engine.Player.AbilityTreeSystem
{
    public class AbilityTreeInterfaceItemInfo : InventoryInterfaceItemInfo
    {
        private readonly Vector2 _targetResolution = new Vector2(1366, 705);
        public AbilityTreeInterfaceItemInfo()
        {
            BackgroundTexture.Scale *= 1.15f;
            ItemTexture.Scale *= .4f;
            ItemTexture.TextureElement.MaskId = InventoryArrayInterface.DefaultId;
        }

        protected override void UpdateView()
        {
            var realSkill = CurrentItem.GetAttribute<AbstractBaseSkill>("Skill");
            var originalLevel = realSkill.Level;
            try
            {
                realSkill.Level = Math.Max(1, originalLevel);
                ItemDescription.Text = TextProvider.Wrap(realSkill.Description, 35);
                ItemDescription.Color = Color.White;
                ItemText.Text = Utils.FitString(realSkill.DisplayName, 18);
                ItemAttributes.Text = TextProvider.Wrap(BuildAttributes(realSkill), 35);
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

        private static string BuildAttributes(AbstractBaseSkill RealSkill)
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
            ItemDescription.Position = Position - Mathf.ScaleGui(_targetResolution, Vector2.UnitY * .025f);
            ItemTexture.Position = Position + Mathf.ScaleGui(_targetResolution, Vector2.UnitY * .2f);
            ItemAttributes.Position = ItemDescription.Position - (ItemDescription.Scale.Y + ItemAttributes.Scale.Y) * Vector2.UnitY - Mathf.ScaleGui(_targetResolution, Vector2.UnitY * .05f);
            SetTitlePosition();
        }
    }
}
