/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 26/04/2016
 * Time: 10:11 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Linq;

namespace Hedra.Engine.ItemSystem
{
	/// <summary>
	/// Description of ItemPool.
	/// </summary>
	public static class ItemPool
	{

	    public static Item Grab(ItemPoolSettings Settings)
	    {
	        var templates = ItemFactory.Templater.Templates;
	        templates = templates.Where(Template => 
            Settings.SameTier ? Template.Tier == Settings.Tier : (int)Template.Tier <= (int)Settings.Tier).ToArray();
	        templates = templates.Where(Template => Template.EquipmentType == Settings.EquipmentType).ToArray();
	        return Item.FromTemplate(templates[Settings.Rng.Next(0, templates.Length)]);
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
