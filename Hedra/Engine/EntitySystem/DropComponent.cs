/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 29/08/2016
 * Time: 08:35 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using OpenTK;
using Hedra.Engine.Item;
using Hedra.Engine.Management;

namespace Hedra.Engine.EntitySystem
{
	/// <summary>
	/// Description of DropComponent.
	/// </summary>
	public class DropComponent : EntityComponent, ITickable
    {
		public bool RandomDrop = true;
		public InventoryItem ItemDrop;
		public int DropChance = 6;
		public DropComponent(Entity Parent) : base(Parent){}
		private bool _dropped = false;
		
		public override void Update(){}
		
		public void Drop()
		{
			if(this.Parent.IsDead && !_dropped){
				_dropped = true;
				if(Utils.Rng.Next(0, DropChance) == 0){
					InventoryItem item = RandomDrop ? new InventoryItem( ItemType.Random ) : ItemDrop;
					if(item != null){
						Generation.World.DropItem(item, Parent.Position + Vector3.UnitY * 2f + new Vector3(Utils.Rng.NextFloat() * 8f -4f, 0, Utils.Rng.NextFloat() * 8f -4f));
					}
				}
			}	
		}
	}
}
