/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 23/07/2017
 * Time: 02:20 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Engine.Player;
using System.Collections.Generic;
using System.Drawing;
using Hedra.Engine.Generation;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player.Inventory;
using OpenTK.Input;

namespace Hedra.Engine.EntitySystem
{
	/// <summary>
	/// Description of MerchantComponent.
	/// </summary>
	public class MerchantComponent : EntityComponent
	{
		public int TradeRadius = 12;
		public Dictionary<int, Item> Items { get; }

		public MerchantComponent(Entity Parent, bool TravellingMerchant) : base(Parent){
			var rng = new Random(World.Seed + 82823 + Utils.Rng.Next(-9999999, 9999999));

		    var items = new []
		    {
		        ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.Axe)),
		        ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.Sword)),
		        ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.Hammer)),
		        ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.Claw)),
		        ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.Katar)),
		        ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.DoubleBlades)),
		        ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.Bow)),
		        ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon, EquipmentType.Knife))
		    };
		    var berry = ItemPool.Grab(ItemType.Berry);
            berry.SetAttribute(CommonAttributes.Amount, int.MaxValue);
            Items = new Dictionary<int, Item>
		    {
                {TradeInventory.MerchantSpaces - 1, berry}
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
		    (Parent as Humanoid).Gold = int.MaxValue;
		}
		 
		public override void Update(){
			var player = LocalPlayer.Instance;
			
			if( (LocalPlayer.Instance.Position - this.Parent.Position).Xz.LengthSquared < 24*24){
        		Parent.Orientation = (LocalPlayer.Instance.Position - Parent.Position).Xz.NormalizedFast().ToVector3();
	            Parent.Model.TargetRotation = Physics.DirectionToEuler( Parent.Orientation );
        	}

		    var canTrade = player.CanInteract && !player.IsDead && !GameSettings.Paused &&
		                   !player.Inventory.Show && !player.SkillSystem.Show;
		    Func<bool> inRadiusFunc = () => (player.Position - Parent.Position).LengthSquared < TradeRadius * TradeRadius &&
		                       !player.Trade.IsTrading;

            var inRadius = inRadiusFunc();

		    if (!canTrade || !inRadius) return;

		    player.MessageDispatcher.ShowMessageWhile("[E] TO TRADE", Color.White, inRadiusFunc);				
		    if(Events.EventDispatcher.LastKeyDown == Key.E) player.Trade.Trade(this.Parent as Humanoid);
		}
	}
}
