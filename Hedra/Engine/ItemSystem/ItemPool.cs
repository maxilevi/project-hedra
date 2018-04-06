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

namespace Hedra.Engine.ItemSystem
{
	/// <summary>
	/// Description of ItemPool.
	/// </summary>
	public static class ItemPool
	{
	    private static readonly EquipmentType[] WeaponEquipmentTypes;
	    private static readonly EquipmentType[] ArmorEquipmentTypes;
	    private static readonly EffectType[] EffectTypes;

        static ItemPool()
	    {
	       WeaponEquipmentTypes =  new[]
	        {
	            EquipmentType.Axe, EquipmentType.Claw, EquipmentType.Bow,
	            EquipmentType.DoubleBlades, EquipmentType.Katar,
	            EquipmentType.Pants, EquipmentType.Knife, EquipmentType.Sword, EquipmentType.Hammer
	        };

	        ArmorEquipmentTypes = new[]
	        {
	            EquipmentType.Boots, EquipmentType.Pants, EquipmentType.Chestplate, EquipmentType.Helmet
	        };

	        EffectTypes = new[]
	        {
	            EffectType.Fire, EffectType.Bleed, EffectType.Freeze,
	            EffectType.Poison, EffectType.Slow, EffectType.Speed
	        };
	    }

	    public static Item Grab(ItemPoolSettings Settings)
	    {
	        var rng = new Random(Settings.Seed);
            var templates = ItemFactory.Templater.Templates;
	        templates = templates.Where(Template => 
            Settings.SameTier ? Template.Tier == Settings.Tier : (int)Template.Tier <= (int)Settings.Tier).ToArray();
	        if(Settings.EquipmentType != null) templates = templates.Where(Template => Template.EquipmentType == Settings.EquipmentType).ToArray();
	        var item = Item.FromTemplate(templates[rng.Next(0, templates.Length)]);
	        item.SetAttribute(CommonAttributes.Seed, Settings.Seed, true);
            return ItemPool.Randomize(item, rng);
	    }

	    public static Item Randomize(Item Item, Random Rng)
	    {
	        var equipmentType = (EquipmentType) Enum.Parse(typeof(EquipmentType), Item.EquipmentType);
            if (WeaponEquipmentTypes.Contains(equipmentType))
            {
                var originalTier = Item.Tier;
                Item = RandomizeTier(Item, Rng);
                var tierChanged = originalTier != Item.Tier;
                Item.SetAttribute(CommonAttributes.Damage, Item.GetAttribute<float>(CommonAttributes.Damage)
                    * (1.0f + (Rng.NextFloat() * (.3f + (tierChanged ? .1f * (int) Item.Tier : .0f) ) - .1f)));
	            Item.SetAttribute(CommonAttributes.AttackSpeed, Item.GetAttribute<float>(CommonAttributes.AttackSpeed)
                    * (1.0f + (Rng.NextFloat() * (.3f + +(tierChanged ? .1f * (int)Item.Tier : .0f)) - .1f)));
	            if (Rng.Next(0, 10) == 1)
	            {
	                Item.SetAttribute(CommonAttributes.EffectType, EffectTypes[Rng.Next(0, EffectTypes.Length)].ToString());
	            }
            }
	        if (ArmorEquipmentTypes.Contains(equipmentType))
	        {

	        }
	        if (EquipmentType.Ring == equipmentType)
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
	        if (newTier < Item.Tier) throw new Exception("Fix this");
	        Item.Tier = newTier;
	        return Item;
	    }

	    public static Item Grab(string Name)
	    {
	        var template = ItemFactory.Templater[Name];
	        return Item.FromTemplate(template);
	    }

        public static Item Grab(ItemType Type)
	    {
	        return ItemPool.Grab(Type.ToString());
	    }

        public static Item Grab(CommonItems Item)
	    {
	        return ItemPool.Grab(Item.ToString());
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
