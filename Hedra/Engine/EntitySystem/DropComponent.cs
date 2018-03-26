﻿/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 29/08/2016
 * Time: 08:35 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using OpenTK;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;

namespace Hedra.Engine.EntitySystem
{
	/// <summary>
	/// Description of DropComponent.
	/// </summary>
	public class DropComponent : EntityComponent, ITickable
	{
	    public bool RandomDrop { get; set; } = true;
	    public bool GoldDrop { get; set; }
        public Item ItemDrop { get; set; }
		public DropComponent(Entity Parent) : base(Parent){}
        private float _dropChance;
        public float DropChance
        {
            get { return _dropChance; }
            set
            {
                _dropChance = value;
                if(_dropChance > 100 || _dropChance < 0)
                    throw new ArgumentException("Drop chance cannot be less than 0 or more than 100.");
            }
        }
        private bool _dropped = false;
		
		public override void Update(){}
		
		public void Drop()
		{
		    if (!this.Parent.IsDead || _dropped) return;
		    _dropped = true;
		    if (!(Utils.Rng.NextFloat() * 100f < DropChance)) return;

		    var item = RandomDrop ? ItemPool.Grab(new ItemPoolSettings(ItemTier.Uncommon)) : ItemDrop;
		    if (GoldDrop)
		    {
		        var gold = ItemPool.Grab(ItemType.Gold);
                gold.SetAttribute(CommonAttributes.Amount, Utils.Rng.Next(1, 4));
                Generation.World.DropItem(gold,
		            Parent.Position + Vector3.UnitY * 2f +
		            new Vector3(Utils.Rng.NextFloat() * 8f - 4f, 0, Utils.Rng.NextFloat() * 8f - 4f));
		    }

		    if (item != null)
		    {
		        Generation.World.DropItem(item,
		            Parent.Position + Vector3.UnitY * 2f +
		            new Vector3(Utils.Rng.NextFloat() * 8f - 4f, 0, Utils.Rng.NextFloat() * 8f - 4f));
		    }
		}
	}
}
