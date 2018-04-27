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
	/// <inheritdoc />
	/// <summary>
	/// Description of MerchantComponent.
	/// </summary>
	public class MerchantComponent : EntityComponent
	{
	    public new Humanoid Parent;
        public Dictionary<int, Item> Items { get; private set; }
	    private readonly Dictionary<int, Item> _originalItems;

        public MerchantComponent(Humanoid Parent, bool TravellingMerchant) : base(Parent){
            this.Parent = Parent;

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
            _originalItems = new Dictionary<int, Item>
		    {
                {TradeInventory.MerchantSpaces - 1, berry}
		    };
		    for (var i = 0; i < 4; i++)
		    {
		        _originalItems.Add(i, items[rng.Next(0, items.Length)]);
		    }

		    if(TravellingMerchant){
		        _originalItems.Add(TradeInventory.MerchantSpaces - 2, ItemPool.Grab("HorseMount"));
                //_originalItems.Add(TradeInventory.MerchantSpaces - 3, ItemPool.Grab("WolfMount"));
                _originalItems.Add(TradeInventory.MerchantSpaces - 3, ItemPool.Grab(ItemType.Glider));
			}
            Items = new Dictionary<int, Item>(_originalItems);
		    this.Parent.Gold = int.MaxValue;
		}

	    public void TransactionComplete()
	    {
	        Items = new Dictionary<int, Item>(_originalItems);
	    }

		public override void Update(){
			var player = LocalPlayer.Instance;
			
			if( (LocalPlayer.Instance.Position - this.Parent.Position).Xz.LengthSquared < 24*24){
        		Parent.Orientation = (LocalPlayer.Instance.Position - Parent.Position).Xz.NormalizedFast().ToVector3();
	            Parent.Model.TargetRotation = Physics.DirectionToEuler( Parent.Orientation );
        	}

		    var canTrade = player.CanInteract && !player.IsDead && !GameSettings.Paused &&
		                   !player.Inventory.Show && !player.AbilityTree.Show;
		    Func<bool> inRadiusFunc = () => (player.Position - Parent.Position).LengthSquared < TradeInventory.TradeRadius * TradeInventory.TradeRadius &&
		                       !player.Trade.IsTrading;

            var inRadius = inRadiusFunc();

		    if (!canTrade || !inRadius) return;

		    player.MessageDispatcher.ShowMessageWhile("[E] TO TRADE", Color.White, inRadiusFunc);
		}
	}
}
