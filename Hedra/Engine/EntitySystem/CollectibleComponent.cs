/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 28/11/2016
 * Time: 07:01 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using Hedra.Core;
using OpenTK;
using Hedra.Engine.WorldBuilding;
using Hedra.Engine.Generation;
using Hedra.Engine.Generation.ChunkSystem;
using Hedra.Engine.Player;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.Management;
using Hedra.Engine.Rendering;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;
using Hedra.Rendering;
using Hedra.Sound;

namespace Hedra.Engine.EntitySystem
{
    /// <summary>
    /// Description of BerryBushComponent.
    /// </summary>
    public class CollectibleComponent : InteractableComponent, ITickable
    {
        private string CollectMessage { get; set; }
        private Item Drop { get; }
        public override float InteractionAngle => .75f;
        public override string Message => "COLLECT";
        public override int InteractDistance => 16;
        
        public CollectibleComponent(IEntity Parent, Item Drop, string CollectMessage) : base(Parent)
        {
            this.Drop = Drop;
            this.CollectMessage = CollectMessage;
        }
        
        public override void Interact(IPlayer Interactee)
        {
            if (!Interactee.Inventory.AddItem(Drop))
            {
                World.DropItem(Drop, this.Parent.Position);
            }            
            var damage = Parent.SearchComponent<DamageComponent>();
            if(damage != null)
            {
                damage.Immune = false;
                damage.Damage(Parent.MaxHealth, Parent, out _, false, false);
            }
            SoundPlayer.PlaySound(SoundType.NotificationSound, Parent.Position);
            Interactee.MessageDispatcher.ShowNotification(CollectMessage, Colors.DarkRed.ToColor(), 3f, false);
            base.Interact(Interactee);
        }
    }
}
