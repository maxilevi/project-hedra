/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 16/06/2017
 * Time: 02:19 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Drawing;
using OpenTK;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Item;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Events;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player;

namespace Hedra.Engine.QuestSystem
{
	/// <summary>
	/// Description of Chest.
	/// </summary>
	public class Chest : BaseStructure, IUpdatable
	{
		private AnimatedModel Model;
		private Animation IdleAnimation;
		private Animation OpenAnimation;
		public InventoryItem Item;
		private Chunk UnderChunk;
		public Func<bool> Condition;
		public event OnItemCollect OnPickup;
	    private CollisionShape _shape;
		
		public Chest(Vector3 Position, InventoryItem Item){
			this.Item = Item;
			this.Model = AnimationModelLoader.LoadEntity("Assets/Chr/ChestIdle.dae");
			this.IdleAnimation = AnimationLoader.LoadAnimation("Assets/Chr/ChestIdle.dae");
			this.OpenAnimation = AnimationLoader.LoadAnimation("Assets/Chr/ChestOpen.dae");
			this.Position = Position;
			
			this.OpenAnimation.Loop = false;
			this.OpenAnimation.Speed = 1.5f;
			this.OpenAnimation.OnAnimationEnd += delegate { 
				ItemModel WorldItem = World.DropItem(Item, this.Position);
				WorldItem.Position = new Vector3(WorldItem.Position.X, WorldItem.Position.Y + .75f * this.Scale.Y, WorldItem.Position.Z);
				WorldItem.OnPickup += delegate(LocalPlayer Player)
				{
				    OnPickup?.Invoke(Player);
				};
			};
			
			this.Model.PlayAnimation(IdleAnimation);
			this.Model.Scale = Vector3.One * 4.5f;
			this.Model.Fog = true;
			UpdateManager.Add(this);
		}
		
		public void Update(){
			if(UnderChunk == null){
				UnderChunk = World.GetChunkAt(this.Position);
				if(UnderChunk == null || !UnderChunk.Initialized){
					this.Dispose();
					return;
				}
			    this.CreateColliders();
			}
			
			if(UnderChunk == null || UnderChunk.Disposed){
				this.Dispose();
				return;
			}
			
			this.Position = new Vector3(this.Position.X, Physics.HeightAtPosition(this.Position), this.Position.Z);
			this.Model.Update();
			
			var player = LocalPlayer.Instance;	

			if(( player.Position - this.Position).LengthSquared < 16*16 && IsClosed && !GameSettings.Paused){
				player.MessageDispatcher.ShowMessage("[E] INTERACT WITH THE CHEST", .25f, Color.White);
				Model.Tint = new Vector4(1.5f,1.5f,1.5f,1);
				if(EventDispatcher.LastKeyDown == OpenTK.Input.Key.E){
					this.Open();
				}
			}
		}

	    private void CreateColliders()
	    {
            if(UnderChunk == null) return;
	        
	        if (_shape != null)
	        {
                UnderChunk.RemoveCollisionShape(_shape);
	        }
	        var shape = AssetManager.LoadCollisionShapes("Assets/Env/Chest.ply", 1, this.Scale)[0];
	        //This will search for "Assets/Env/Colliders/Chest_Collider0.ply"
            shape.Transform(-Vector3.UnitX * 1.5f);
	        shape.Transform(Matrix4.CreateRotationY(this.Rotation.Y * Mathf.Radian) *
	                        Matrix4.CreateRotationX(this.Rotation.X * Mathf.Radian) *
	                        Matrix4.CreateRotationZ(this.Rotation.Z * Mathf.Radian));

            shape.Transform(this.Position);
	        _shape = shape;
	        UnderChunk.AddCollisionShape(shape);  
	    }
		
		public void Open(){
			if(IsClosed){//Check if it's closed
				
				if(Condition != null)
					if(!Condition()) return;
				
				Model.PlayAnimation(OpenAnimation);
			}
		}
		
		public override void Dispose(){
			UpdateManager.Remove(this);
			this.Model.Dispose();
		}
		
		public bool IsClosed => Model.Animator.AnimationPlaying == IdleAnimation;

	    public new Vector3 Position{
			get{ return Model.Position;  }
	        set
	        {
                if(value == this.Position) return;

	            this.Model.Position = value;
	            this.CreateColliders();
	        }
		}
		
		public Vector3 Scale{
			get{ return Model.Scale;  }
			set{ this.Model.Scale = value; }
		}
		
		public Vector3 Rotation{
			get{ return Model.Rotation;  }
		    set
		    {
		        this.Model.Rotation = value;
		        this.CreateColliders();
		    }
		}
	}
}
