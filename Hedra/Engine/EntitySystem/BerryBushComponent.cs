/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 28/11/2016
 * Time: 07:01 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.QuestSystem;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Player;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Sound;

namespace Hedra.Engine.EntitySystem
{
	/// <summary>
	/// Description of BerryBushComponent.
	/// </summary>
	public class BerryBushComponent : InteractableComponent, ITickable
    {
		public BerryBushComponent(Entity Parent) : base(Parent) { }

        public override float InteractionAngle => .75f;
        public override string Message => "COLLECT";
        public override int InteractDistance => 16;
		
		public override void Interact(LocalPlayer Interactee)
        {
			var berry = ItemPool.Grab(ItemType.Berry);
		    if (!Interactee.Inventory.AddItem(berry))
		    {
		        World.DropItem(berry, this.Parent.Position);
		    }
			
			var damage = Parent.SearchComponent<DamageComponent>();
			if(damage != null)
            {
				damage.Immune = false;
				damage.Damage(Parent.MaxHealth, Parent, out float xp, false);
			}
            SoundManager.PlaySound(SoundType.NotificationSound, Parent.Position);
            Interactee.MessageDispatcher.ShowNotification("You got a berry from the bush", Colors.DarkRed.ToColor(), 3f, false);
            base.Interact(Interactee);
        }
    }
}
