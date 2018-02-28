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
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Item;

namespace Hedra.Engine.EntitySystem
{
	/// <summary>
	/// Description of BerryBushComponent.
	/// </summary>
	public class BerryBushComponent : EntityComponent, IClaimable, IDisposable
	{
		public Chunk UnderChunk;
		private bool _interacted = false;

		public BerryBushComponent(Entity Parent) : base(Parent){}
		
		public override void Update(){
			
			LocalPlayer Player = Scenes.SceneManager.Game.LPlayer;
			Parent.Model.Tint = new Vector4(1f,1f,1f,1);
			if( (Parent.Position - Player.Position).LengthSquared < 16*16 
			   && Vector3.Dot( (Parent.Position - Player.Position).NormalizedFast(), Player.View.LookAtPoint.NormalizedFast()) > .6f ){
			    Player.MessageDispatcher.ShowMessage("[E] COLLECT", .5f);
				Parent.Model.Tint = new Vector4(1.5f,1.5f,1.5f,1);
			}
		}
		
		public void Interact(LocalPlayer Player){
			if(_interacted) return;
			if( (Parent.Position - Player.Position).LengthSquared > 16*16 
			   || Vector3.Dot( (Parent.Position - Player.Position).NormalizedFast(), Player.View.LookAtPoint.NormalizedFast()) < .6f )return;
			
			var berry = new InventoryItem(ItemType.Food, ItemInfo.Berry(1) );
			Player.Inventory.AddItem(berry);
			Sound.SoundManager.PlaySound(Sound.SoundType.NotificationSound, Parent.Position);
			Player.MessageDispatcher.ShowNotification("You got a berry from the bush", System.Drawing.Color.DarkRed, 3f, false);
			_interacted = true;
			
			var damage = Parent.SearchComponent<DamageComponent>();
			if(damage != null){
				damage.Immune = false;
				float Exp;
				//Trigger it's dissapeareance.
				damage.Damage(Parent.MaxHealth, Parent, out Exp, false);
			}
		}
	}
}
