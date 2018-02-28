/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 05/05/2016
 * Time: 09:31 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using OpenTK;
using Hedra.Engine.Generation;
using Hedra.Engine.Rendering;
using Hedra.Engine.Item;
using Hedra.Engine.Player;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;

namespace Hedra.Engine.EntitySystem
{
	public delegate void OnItemCollect(LocalPlayer Player);
	public class ItemModel : Model, IUpdatable
	{
		private static ushort ItemCounter = 0;
		
		public ushort ItemId {get; set;}
		public EntityMesh Mesh;
		public InventoryItem Item;
		public event OnItemCollect OnPickup;
		private bool IsColliding;
	
		public ItemModel(InventoryItem Item, Vector3 Position)
		{
			if(Item.MeshFile == null)
			{
				return;
			}
			this.Scale = new Vector3(2.25f, 2.25f, 2.25f);
			this.Item = Item;
			this.Mesh = EntityMesh.FromVertexData(Item.MeshFile.Clone());
		    this.Mesh.Scale = this.Scale;
			this.Position = new Vector3(Position.X, Physics.HeightAtPosition(Position.X, Position.Z) + 1.5f, Position.Z);
			this.ItemId = ++ItemCounter;
			UpdateManager.Add(this);
		}
		
		public override void Update(){
			this.Mesh.TargetRotation += Vector3.UnitY * 35 * (float) Time.deltaTime;
			
			float Dot = Vector3.Dot(-(Scenes.SceneManager.Game.LPlayer.Position - this.Position).NormalizedFast(), Scenes.SceneManager.Game.LPlayer.View.LookAtPoint.NormalizedFast());
			if( Dot > .5f && (this.Position - Scenes.SceneManager.Game.LPlayer.Position).LengthSquared < 12f*12f){
			    LocalPlayer.Instance.MessageDispatcher.ShowMessage("[E] TO PICK UP", .5f);
				Mesh.Tint = new Vector4(1.5f,1.5f,1.5f,1);
				if(Events.EventDispatcher.LastKeyDown == OpenTK.Input.Key.E && !Disposed)
				{
				    OnPickup?.Invoke(Scenes.SceneManager.Game.LPlayer);
				}
			}else{
				Mesh.Tint = new Vector4(1,1,1,1);
			}
		}
		
		public new void Dispose(){
			UpdateManager.Remove(this);
			World.Items.Remove(this);
			base.Dispose();
		}
		
		public override void Run(){}
		public override void Idle(){}
	}
}
