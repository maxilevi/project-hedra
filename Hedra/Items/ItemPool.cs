/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 26/04/2016
 * Time: 10:11 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Linq;
using Hedra.Core;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ItemSystem.Templates;

namespace Hedra.Items
{
    /// <summary>
    /// Description of ItemPool.
    /// </summary>
    public static class ItemPool
    {
        private static readonly EquipmentType[] WeaponEquipmentTypes;
        private static readonly EquipmentType[] ArmorEquipmentTypes;
        private static readonly string[] BlacklistedEquipment;
        private static readonly string[] BlacklistedItems;
        private static readonly EffectType[] EffectTypes;

        static ItemPool()
        {
           WeaponEquipmentTypes =  new[]
            {
                EquipmentType.Axe, EquipmentType.Claw, EquipmentType.Bow,
                EquipmentType.DoubleBlades, EquipmentType.Katar, EquipmentType.Staff,
                EquipmentType.Knife, EquipmentType.Sword, EquipmentType.Hammer
            };

            ArmorEquipmentTypes = new[]
            {
                EquipmentType.Boots, EquipmentType.Pants,
                EquipmentType.Chestplate, EquipmentType.Helmet
            };

            EffectTypes = new[]
            {
                EffectType.Fire, EffectType.Bleed, EffectType.Freeze,
                EffectType.Poison, EffectType.Slow, EffectType.Speed
            };

            BlacklistedItems = new[]
            {
                ItemType.Gold.ToString()
            };
            
            BlacklistedEquipment = new[]
            {
                EquipmentType.Chestplate.ToString()
            };
        }

        public static Item Grab(ItemPoolSettings Settings)
        {
            var rng = new Random(Settings.Seed);
            var templates = ItemLoader.Templater.Templates;
            var selectedTier = Settings.RandomizeTier ? Settings.Tier : SelectTier(Settings.Tier, rng);

            var newTemplates = templates.Where(Template =>
                Settings.SameTier ? Template.Tier == selectedTier : Template.Tier <= selectedTier).ToArray();

            if (Settings.EquipmentType != null)
            {
                newTemplates = newTemplates.Where(Template => Template.EquipmentType == Settings.EquipmentType).ToArray();
            }

            if (newTemplates.Length == 0 && Settings.EquipmentType != null)
            {
                newTemplates = templates.Where(Template => Template.Tier <= selectedTier 
                && Template.EquipmentType == Settings.EquipmentType).ToArray();
            }
            templates = newTemplates
                .Where(Template => Array.IndexOf(BlacklistedEquipment, Template.EquipmentType) == -1)
                .Where(Template => Array.IndexOf(BlacklistedItems, Template.Name) == -1)
                .ToArray();
            if (templates.Length == 0) throw new ArgumentOutOfRangeException($"No valid item template found.");
            
            var item = Item.FromTemplate(templates[rng.Next(0, templates.Length)]);
            return Grab(item.Name, Settings.Seed);
        }

        public static ItemTier SelectTier(ItemTier Tier, Random Rng)
        {
            if (Tier == 0) return Tier;

            var selectedTier = Tier;
            for (var i = (int)Tier-1; i > -1; i--)
            {
                var useThisTier = true;
                for (var k = 0; k < (int) Tier-i+1; k++)
                {
                    useThisTier = useThisTier && Rng.Next(0, 4) == 1;
                }
                if (!useThisTier) continue;
                selectedTier = (ItemTier) i;
                break;
            }
            return selectedTier;
        }

        public static Item Randomize(Item Item, Random Rng)
        {
            var isBuiltin = Enum.TryParse<EquipmentType>(Item.EquipmentType, true, out var equipmentType);
            if (WeaponEquipmentTypes.Contains(equipmentType))
            {
                var originalTier = Item.Tier;
                Item = RandomizeTier(Item, Rng);
                var tierChanged = originalTier != Item.Tier;
                Item.SetAttribute(CommonAttributes.Damage, Item.GetAttribute<float>(CommonAttributes.Damage)
                    * (1.0f + (Rng.NextFloat() * (.1f + (tierChanged ? .1f * (int) Item.Tier : .0f) ) - .075f)));
                Item.SetAttribute(CommonAttributes.AttackSpeed, Item.GetAttribute<float>(CommonAttributes.AttackSpeed)
                    * (1.0f + (Rng.NextFloat() * (.1f + (tierChanged ? .1f * (int)Item.Tier : .0f)) - .075f)));
                if ( Item.Tier > ItemTier.Common && Rng.Next(0, 10) == 1)
                {
                    Item.SetAttribute(CommonAttributes.EffectType, EffectTypes[Rng.Next(0, EffectTypes.Length)].ToString());
                }
            }
            if (isBuiltin && ArmorEquipmentTypes.Contains(equipmentType))
            {

            }
            if (isBuiltin && EquipmentType.Ring == equipmentType)
            {
                Item.SetAttribute(CommonAttributes.MovementSpeed, Item.GetAttribute<float>(CommonAttributes.MovementSpeed) * (1.0f + (Rng.NextFloat() * .3f - .15f)));
                Item.SetAttribute(CommonAttributes.AttackSpeed, Item.GetAttribute<float>(CommonAttributes.AttackSpeed) * (1.0f + (Rng.NextFloat() * .3f - .15f)));
                Item.SetAttribute(CommonAttributes.Health, Item.GetAttribute<float>(CommonAttributes.Health) * (1.0f + (Rng.NextFloat() * .3f - .15f)));
            }
            return Item;
        }

        private static Item RandomizeTier(Item Item, Random Rng)
        {
            var newTier = Item.Tier;
            for (var i = (int)ItemTier.Misc - 1; i > -1; i--)
            {
                if (i < (int)Item.Tier) break;
                var shouldConvert = true;
                for (var k = 0; k < i; k++)
                {
                    shouldConvert = Rng.NextBool() && Rng.NextBool();
                    if(!shouldConvert) break;
                }
                if (shouldConvert)
                {
                    newTier = (ItemTier)i;
                    break;
                }
            }
            Item.Tier = newTier;
            return Item;
        }

        public static Item[] Matching(Predicate<ItemTemplate> Searcher)
        {
            return ItemLoader.Templater.Templates.Where(I => Searcher(I)).Select(I => ItemPool.Grab(I.Name)).ToArray();
        }

        public static Item Grab(string Name)
        {
            return Item.FromTemplate(ItemLoader.Templater[Name]);
        }
        
        public static Item Grab(string Name, int Seed)
        {
            var item = Item.FromTemplate(ItemLoader.Templater[Name]);
            item.SetAttribute(CommonAttributes.Seed, Seed, true);
            return ItemPool.Randomize(item, new Random(Seed));
        }

        public static Item Grab(ItemType Type)
        {
            return Grab(Type.ToString());
        }
        
        public static Item Grab(ItemTier Tier)
        {
            return Grab(new ItemPoolSettings(Tier));
        }

        public static Item Grab(CommonItems Item)
        {
            return Grab(Item.ToString());
        }

        public static bool Exists(string Name)
        {
            return ItemLoader.Templater.Contains(Name);
        }

        public static void Load(params ItemTemplate[] Templates)
        {
            ItemLoader.Instance.Load(Templates);
        }
    }

    public enum EffectType{
        None,
        Bleed,
        Fire,
        Poison,
        Freeze,
        Speed,
        Slow,
        MaxItems
    }
}
