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
	        var templates = ItemFactory.Templater.Templates;
	        templates = templates.Where(Template => 
            Settings.SameTier ? Template.Tier == Settings.Tier : (int)Template.Tier <= (int)Settings.Tier).ToArray();
	        if(Settings.EquipmentType != null) templates = templates.Where(Template => Template.EquipmentType == Settings.EquipmentType).ToArray();
	        return ItemPool.Randomize(Settings, Item.FromTemplate(templates[Settings.Rng.Next(0, templates.Length)]));
	    }

	    private static Item Randomize(ItemPoolSettings Settings, Item Item)
	    {
	        var equipmentType = (EquipmentType) Enum.Parse(typeof(EquipmentType), Item.EquipmentType);
            if (WeaponEquipmentTypes.Contains(equipmentType))
	        {
	            Item.SetAttribute(CommonAttributes.Damage, Item.GetAttribute<float>(CommonAttributes.Damage) * (1.0f + (Settings.Rng.NextFloat() * .3f - .15f) ));
	            Item.SetAttribute(CommonAttributes.AttackSpeed, Item.GetAttribute<float>(CommonAttributes.AttackSpeed) * (1.0f + (Settings.Rng.NextFloat() * .3f - .15f)));
	            if (Settings.Rng.Next(1, 2) == 1)
	            {
	                Item.SetAttribute(CommonAttributes.EffectType, EffectTypes[Settings.Rng.Next(0, EffectTypes.Length)].ToString());
	            }
            }
	        if (ArmorEquipmentTypes.Contains(equipmentType))
	        {

	        }
	        if (EquipmentType.Ring == equipmentType)
	        {
	            Item.SetAttribute(CommonAttributes.MovementSpeed, Item.GetAttribute<float>(CommonAttributes.MovementSpeed) * (1.0f + (Settings.Rng.NextFloat() * .3f - .15f)));
	            Item.SetAttribute(CommonAttributes.AttackSpeed, Item.GetAttribute<float>(CommonAttributes.AttackSpeed) * (1.0f + (Settings.Rng.NextFloat() * .3f - .15f)));
	            Item.SetAttribute(CommonAttributes.Health, Item.GetAttribute<float>(CommonAttributes.Health) * (1.0f + (Settings.Rng.NextFloat() * .3f - .15f)));
            }
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
