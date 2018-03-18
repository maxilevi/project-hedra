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
using System.Collections.Generic;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.ItemSystem.WeaponSystem;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.EntitySystem
{
	/// <summary>
	/// Description of MerchantComponent.
	/// </summary>
	public class MerchantComponent : EntityComponent
	{
		public int TradeRadius = 12;
		public Dictionary<int, Item> Items;

		public MerchantComponent(Entity Parent, bool TravellingMerchant) : base(Parent){
			var rng = new Random(World.Seed + 82823 + Utils.Rng.Next(-9999999, 9999999));

		    var items = new []
		    {
		        ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, WeaponType.Axe)),
		        ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, WeaponType.Sword)),
		        ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, WeaponType.Hammer)),
		        ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, WeaponType.Claw)),
		        ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, WeaponType.Katar)),
		        ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, WeaponType.DoubleBlades)),
		        ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, WeaponType.Bow)),
		        ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, WeaponType.Knife))
		    };
		    Items = new Dictionary<int, Item>
		    {
                {TradeSystem.MaxItems - 1, ItemPool.Grab(ItemType.Berry)}
		    };
		    for (var i = 0; i < 4; i++)
		    {
		        Items.Add(i, items[rng.Next(0, items.Length)]);
		    }

		    if(TravellingMerchant){
				//Items.Add(TradeSystem.MaxItems - 2, ItemPool.Grab("HorseMount"));
				//Items.Add(TradeSystem.MaxItems - 3, ItemPool.Grab("WolfMount"));
				//Items.Add(TradeSystem.MaxItems - 4, ItemPool.Grab(ItemType.Glider));
			}
		}
		 
		public override void Update(){
			LocalPlayer Player = LocalPlayer.Instance;
			
			if( (LocalPlayer.Instance.Position - this.Parent.Position).Xz.LengthSquared < 24*24){
        		Parent.Orientation = (LocalPlayer.Instance.Position - Parent.Position).Xz.NormalizedFast().ToVector3();
	            Parent.Model.TargetRotation = Physics.DirectionToEuler( Parent.Orientation );
        	}
			/*
			if( Player.CanInteract && !Player.IsDead && !GameSettings.Paused && !Player.Trade.Show && !Player.Inventory.Show && !Player.SkillSystem.Show && (Player.Position - Parent.Position).LengthSquared < TradeRadius*TradeRadius){
			    Player.MessageDispatcher.ShowMessageWhile("[E] TO TRADE", Color.White,
				    () => (Player.Position - Parent.Position).LengthSquared < TradeRadius * TradeRadius && !Player.Trade.Show);
				
				if(Events.EventDispatcher.LastKeyDown == Key.E){
					Player.Trade.Show = true;
					Player.Trade.SetMerchantItems(this.Items);
				}
			}*/
		}
	}
}
