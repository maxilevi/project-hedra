/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/07/2017
 * Time: 02:20 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK.Input;
using System.Drawing;
using Hedra.Engine.Player;
using Hedra.Engine.Item;
using System.Collections.Generic;
using Hedra.Engine.Generation;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.EntitySystem
{
	/// <summary>
	/// Description of MerchantComponent.
	/// </summary>
	public class MerchantComponent : EntityComponent
	{
		public int TradeRadius = 12;
		public Dictionary<InventoryItem, int> Items;
		public MerchantComponent(Entity Parent, bool TravellingMerchant) : base(Parent){
			var Rng = new Random(World.Seed + 82823 + Utils.Rng.Next(-9999999, 9999999));
			
			Items = new Dictionary<InventoryItem, int>();
			Items.Add(new InventoryItem(ItemType.Axe, ItemInfo.Random(ItemType.Axe, Rng)), 0);
			Items.Add(new InventoryItem(ItemType.Sword, ItemInfo.Random(ItemType.Sword, Rng)), 1);
			Items.Add(new InventoryItem(ItemType.Hammer, ItemInfo.Random(ItemType.Hammer, Rng)), 2);
			Items.Add(new InventoryItem(ItemType.Claw, ItemInfo.Random(ItemType.Claw, Rng)), 3);
			Items.Add(new InventoryItem(ItemType.Katar, ItemInfo.Random(ItemType.Katar, Rng)), 4);
			Items.Add(new InventoryItem(ItemType.DoubleBlades, ItemInfo.Random(ItemType.DoubleBlades, Rng)), 5);
			Items.Add(new InventoryItem(ItemType.Bow, ItemInfo.Random(ItemType.Bow, Rng)), 6);
			Items.Add(new InventoryItem(ItemType.Knife, ItemInfo.Random(ItemType.Knife, Rng)), 7);
			
			Items.Add(new InventoryItem(ItemType.Food, Item.ItemInfo.Berry(1)), TradeSystem.MaxItems-4);
			if(TravellingMerchant){
				Items.Add(new InventoryItem(ItemType.Mount, new ItemInfo(Material.HorseMount, 200f)), TradeSystem.MaxItems-3);
				Items.Add(new InventoryItem(ItemType.Mount, new ItemInfo(Material.WolfMount, 100f)), TradeSystem.MaxItems-2);
				Items.Add(new InventoryItem(ItemType.Glider, ItemInfo.Random(ItemType.Glider)), TradeSystem.MaxItems-1);
			}
		}
		 
		public override void Update(){
			LocalPlayer Player = LocalPlayer.Instance;
			
			if( (LocalPlayer.Instance.Position - this.Parent.Position).Xz.LengthSquared < 24*24){
        		Parent.Orientation = (LocalPlayer.Instance.Position - Parent.Position).Xz.NormalizedFast().ToVector3();
	            Parent.Model.TargetRotation = Physics.DirectionToEuler( Parent.Orientation );
        	}
			
			if( Player.CanInteract && !Player.IsDead && !GameSettings.Paused && !Player.Trade.Show && !Player.Inventory.Show && !Player.SkillSystem.Show && (Player.Position - Parent.Position).LengthSquared < TradeRadius*TradeRadius){
			    Player.MessageDispatcher.ShowMessageWhile("[E] TO TRADE", Color.White,
				    () => (Player.Position - Parent.Position).LengthSquared < TradeRadius * TradeRadius && !Player.Trade.Show);
				
				if(Events.EventDispatcher.LastKeyDown == Key.E){
					Player.Trade.Show = true;
					Player.Trade.SetMerchantItems(this.Items);
				}
			}
		}
	}
}
