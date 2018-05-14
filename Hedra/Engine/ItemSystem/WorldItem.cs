/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 05/05/2016
 * Time: 09:31 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Runtime.Remoting.Messaging;
using Hedra.Engine.Events;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.PhysicsSystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.ItemSystem
{
	public delegate void OnItemCollect(LocalPlayer Player);

	public class WorldItem : Model, IUpdatable
	{
		private static ushort _itemCounter;
		public bool PickedUp { get; private set; }
		public ushort ItemId {get; set;}
		public ObjectMesh Mesh;
		public Item ItemSpecification;
		public event OnItemCollect OnPickup;
	    private readonly float _height;
        private bool _isColliding;
	    private bool _shouldPickup;
	    private bool _canPickup;

        public WorldItem(Item ItemSpecification, Vector3 Position)
		{
			this.Scale = new Vector3(1.5f, 1.5f, 1.5f);
			this.ItemSpecification = ItemSpecification;
		    var modelData = ItemSpecification.Model.Clone();
		    this._height = Math.Abs(modelData.SupportPoint(-Vector3.UnitY).Y - modelData.SupportPoint(Vector3.UnitY).Y) - 1f;
            this.Mesh = ObjectMesh.FromVertexData(modelData);
		    this.Mesh.OutlineColor = ItemUtils.TierToColor(ItemSpecification.Tier).ToVector4();
            this.Mesh.BaseTint = EffectDescriber.EffectColorFromItem(ItemSpecification);
		    this.Mesh.Scale = this.Scale;
		    this.Position = Position;
			this.ItemId = ++_itemCounter;
		    this.OnPickup += Player => PickedUp = true;

		    EventDispatcher.RegisterKeyDown(this, delegate(Object Sender, KeyboardKeyEventArgs EventArgs)
		    {
		        if (_canPickup && Key.E == EventArgs.Key) _shouldPickup = true;
		    });
		    var shadow = new DropShadow
		    {
		        Position = Position - Vector3.UnitY * 1f,
		        DepthTest = true,
		        DeleteWhen = () => this.Disposed,
                Rotation = new Matrix3(Mathf.RotationAlign(Vector3.UnitY, Physics.NormalAtPosition(Position))),
                IsReplacementShadow = true
		    };
            DrawManager.DropShadows.Add(shadow);

			UpdateManager.Add(this);
		}
		
		public override void Update(){
		    this.Position = new Vector3(Position.X, Physics.HeightAtPosition(Position.X, Position.Z) + _height + (float) Math.Cos( Time.CurrentFrame), Position.Z);
            this.Mesh.TargetRotation += Vector3.UnitY * 35f * (float) Time.deltaTime;

		    float DotFunc() => Vector2.Dot((this.Position - GameManager.Player.Position).Xz.NormalizedFast(), LocalPlayer.Instance.View.LookingDirection.Xz.NormalizedFast());

		    if(DotFunc() > .9f && (this.Position - LocalPlayer.Instance.Position).LengthSquared < 14f*14f){
			    LocalPlayer.Instance.MessageDispatcher.ShowMessageWhile("[E] TO PICK UP", 
                    () => !Disposed && DotFunc() > .9f && (this.Position - LocalPlayer.Instance.Position).LengthSquared < 14f * 14f);
			    _canPickup = true;

				if(LocalPlayer.Instance.Inventory.HasAvailableSpace && _shouldPickup && !PickedUp && !Disposed)
				{
				    OnPickup?.Invoke(GameManager.Player);
				}
			}else
			{
			    _canPickup = false;
			}
		    Mesh.Tint = _canPickup ? Vector4.One * 1.5f : Vector4.One;
		    this.Mesh.Outline = true;//_canPickup;
        }
		
		public new void Dispose(){
            EventDispatcher.UnregisterKeyDown(this);
			UpdateManager.Remove(this);
			World.RemoveItem(this);
			base.Dispose();
		}
		
		public override void Run(){}
		public override void Idle(){}
	}
}
