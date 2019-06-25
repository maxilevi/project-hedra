using Hedra.Components.Effects;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Scripting;
using Hedra.Rendering;

namespace Hedra.Items
{
    public class PotionHandler : ItemHandler
    {
        public override bool Consume(IPlayer Owner, Item Item)
        {
            if (Item.Name == PotionType.ManaPotion.ToString())
            {
                Owner.Mana += Item.GetAttribute<int>(PotionAttributes.PotionBonus.ToString());
                VisualEffects.Outline(Owner, Colors.LightBlue, 1);
                return true;
            }

            if (Item.Name == PotionType.SpeedPotion.ToString())
            {
                Owner.AddBonusSpeedForSeconds(
                    Owner.Speed * Item.GetAttribute<float>(PotionAttributes.PotionBonus.ToString()),
                    Item.GetAttribute<int>(PotionAttributes.PotionDuration.ToString())
                );
                VisualEffects.Outline(Owner, Colors.White, 1);
                return true;
            }

            if (Item.Name == PotionType.StaminaPotion.ToString())
            {
                Owner.AddComponentForSeconds(
                    new StaminaRegenComponent(Owner, Owner.StaminaRegen * Item.GetAttribute<float>(PotionAttributes.PotionBonus.ToString())),
                    Item.GetAttribute<int>(PotionAttributes.PotionDuration.ToString())
                );
                VisualEffects.Outline(Owner, Colors.Yellow, 1);
                return true;
            }

            if (Item.Name == PotionType.StrengthPotion.ToString())
            {
                Owner.AddComponentForSeconds(
                    new AttackPowerBonusComponent(Owner, Owner.AttackPower * Item.GetAttribute<float>(PotionAttributes.PotionBonus.ToString())),
                    Item.GetAttribute<int>(PotionAttributes.PotionDuration.ToString())
                );
                VisualEffects.Outline(Owner, Colors.Magenta, 1);
                return true;
            }

            return false;
        }
    }
}