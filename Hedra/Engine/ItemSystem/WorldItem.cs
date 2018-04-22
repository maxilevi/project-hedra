/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 05/05/2016
 * Time: 09:31 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System.Runtime.Remoting.Messaging;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using OpenTK;

namespace Hedra.Engine.ItemSystem
{
	public delegate void OnItemCollect(LocalPlayer Player);

	public class WorldItem : Model, IUpdatable
	{
		private static ushort _itemCounter;
		
		public ushort ItemId {get; set;}
		public ObjectMesh Mesh;
		public Item ItemSpecification;
		public event OnItemCollect OnPickup;
		private bool _isColliding;
	
		public WorldItem(Item ItemSpecification, Vector3 Position)
		{
			this.Scale = new Vector3(2.25f, 2.25f, 2.25f);
			this.ItemSpecification = ItemSpecification;
			this.Mesh = ObjectMesh.FromVertexData(ItemSpecification.Model.Clone());
		    this.Mesh.BaseTint = EffectDescriber.EffectColorFromItem(ItemSpecification);
		    this.Mesh.Scale = this.Scale;
			this.Position = new Vector3(Position.X, Physics.HeightAtPosition(Position.X, Position.Z) + 1.5f, Position.Z);
			this.ItemId = ++_itemCounter;
			UpdateManager.Add(this);
		}
		
		public override void Update(){
			this.Mesh.TargetRotation += Vector3.UnitY * 35 * (float) Time.deltaTime;
			
			float dot = Vector3.Dot(-(LocalPlayer.Instance.Position - this.Position).NormalizedFast(),
                LocalPlayer.Instance.View.LookAtPoint.NormalizedFast());

			if( dot > .65f && (this.Position - LocalPlayer.Instance.Position).LengthSquared < 12f*12f){
			    LocalPlayer.Instance.MessageDispatcher.ShowMessageWhile("[E] TO PICK UP", 
                    () => !Disposed && Vector3.Dot(-(LocalPlayer.Instance.Position - this.Position).NormalizedFast(),
			                                                                                        LocalPlayer.Instance.View.LookAtPoint.NormalizedFast()) > .65f && (this.Position - LocalPlayer.Instance.Position).LengthSquared < 12f * 12f);
				Mesh.Tint = new Vector4(1.5f,1.5f,1.5f,1);
				if(LocalPlayer.Instance.Inventory.HasAvailableSpace && Events.EventDispatcher.LastKeyDown == OpenTK.Input.Key.E && !Disposed)
				{
				    OnPickup?.Invoke(GameManager.Player);
				}
			}else{
				Mesh.Tint = new Vector4(1,1,1,1);
			}
		}
		
		public new void Dispose(){
			UpdateManager.Remove(this);
			World.RemoveItem(this);
			base.Dispose();
		}
		
		public override void Run(){}
		public override void Idle(){}
	}
}
