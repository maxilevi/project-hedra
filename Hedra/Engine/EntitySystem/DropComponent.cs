/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 29/08/2016
 * Time: 08:35 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Item;
using Hedra.Engine.Rendering;
using Hedra.Engine.Player;
using Hedra.Engine.Management;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Hedra.Engine.EntitySystem
{
	/// <summary>
	/// Description of DropComponent.
	/// </summary>
	public class DropComponent : EntityComponent
	{
		public bool RandomDrop = true;
		public InventoryItem ItemDrop;
		public int DropChance = 6;
		public DropComponent(Entity Parent) : base(Parent){}
		
		private bool Dropped = false;
		
		public override void Update(){}
		
		public void Drop()
		{
			if(this.Parent.IsDead && !Dropped){
				Dropped = true;
				if(Utils.Rng.Next(0, DropChance) == 0){
					InventoryItem Item = (RandomDrop) ? new InventoryItem( ItemType.Random ) : ItemDrop;
					if(Item != null){
						Engine.Generation.World.DropItem(Item, Parent.Position + Vector3.UnitY * 2f + new Vector3(Utils.Rng.NextFloat() * 8f -4f, 0, Utils.Rng.NextFloat() * 8f -4f));
					}
				}
			}	
		}
	}
}
