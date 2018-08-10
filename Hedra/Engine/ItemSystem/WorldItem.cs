/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 05/05/2016
 * Time: 09:31 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Drawing;
using Hedra.Engine.EntitySystem;
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
	public delegate void OnItemCollect(IPlayer Player);

	public class WorldItem : UpdatableModel<ObjectMesh>, IUpdatable
	{
        private static ushort _itemCounter;
		public bool PickedUp { get; private set; }
		public ushort ItemId { get; }
		public Item ItemSpecification { get; }
		public event OnItemCollect OnPickup;
	    private readonly float _height;
        private bool _isColliding;
	    private bool _shouldPickup;
	    private bool _canPickup;

        public WorldItem(Item ItemSpecification, Vector3 Position) : base(null)
		{
            var modelData = ItemSpecification.Model.Clone();
		    this.ItemSpecification = ItemSpecification;
		    this.ItemId = ++_itemCounter;
            this.Model = ObjectMesh.FromVertexData(modelData);
		    this.Model.OutlineColor = ItemUtils.TierToColor(ItemSpecification.Tier).ToVector4();
            this.Model.BaseTint = EffectDescriber.EffectColorFromItem(ItemSpecification);
		    this.Scale = new Vector3(1.5f, 1.5f, 1.5f);
		    this.Position = Position;
            this._height = Math.Abs(modelData.SupportPoint(-Vector3.UnitY).Y - modelData.SupportPoint(Vector3.UnitY).Y);
		    this.OnPickup += Player => PickedUp = true;

		    EventDispatcher.RegisterKeyDown(this, delegate(Object Sender, KeyEventArgs EventArgs)
		    {
		        if (_canPickup && Key.E == EventArgs.Key) _shouldPickup = true;
		    });
		    var shadow = new DropShadow
		    {
		        Position = Position - Vector3.UnitY * 1.5f,
		        DepthTest = true,
		        DeleteWhen = () => this.Disposed,
                Rotation = new Matrix3(Mathf.RotationAlign(Vector3.UnitY, Physics.NormalAtPosition(Position))),
                IsCosmeticShadow = true,
                Opacity = .5f
		    };
            DrawManager.DropShadows.Add(shadow);
            
			UpdateManager.Add(this);
		}
		
		public override void Update()
		{
		    this.Model.Alpha = this.Alpha;
            if(this.PickedUp) return;

            var heightAtPosition = Physics.HeightAtPosition(Position.X, Position.Z);
		    this.Position = new Vector3(Position.X,
		        Math.Max(heightAtPosition + _height, heightAtPosition + _height + (float) Math.Cos(Time.AccumulatedFrameTime)),
		        Position.Z);
		    this.Model.TargetRotation += Vector3.UnitY * 35f * (float) Time.DeltaTime;
		    
		    float DotFunc() => Vector2.Dot((this.Position - GameManager.Player.Position).Xz.NormalizedFast(), LocalPlayer.Instance.View.LookingDirection.Xz.NormalizedFast());

		    if(DotFunc() > .9f && (this.Position - LocalPlayer.Instance.Position).LengthSquared < 14f*14f){
			    LocalPlayer.Instance.MessageDispatcher.ShowMessageWhile("[E] TO PICK UP", 
                    () => !Disposed && DotFunc() > .9f && (this.Position - LocalPlayer.Instance.Position).LengthSquared < 14f * 14f);
			    _canPickup = true;

		        if (LocalPlayer.Instance.Inventory.HasAvailableSpace && _shouldPickup && !PickedUp && !Disposed)
		        {
		            OnPickup?.Invoke(GameManager.Player);
		        }
		        else
		        {
		            if (_shouldPickup)
		            {
		                _shouldPickup = false;
                        GameManager.Player.MessageDispatcher.ShowNotification("YOUR INVENTORY IS FULL", Color.Red, 3f, true);
		            }
		        }
			}else
			{
			    _canPickup = false;
			}
		    this.Model.Tint = _canPickup ? Vector4.One * 1.5f : Vector4.One;
		    this.Model.Outline = true;

		    if (!ItemSpecification.IsEquipment && (this.Position - GameManager.Player.Position).Xz.LengthSquared < 12 * 12 
                && GameManager.Player.Inventory.Search(I => I.Name == ItemSpecification.Name) != null)
		    {
		        if (!PickedUp && !Disposed)
		        {
		            OnPickup?.Invoke(GameManager.Player);
                }
            }
        }
		
		public bool Outline
		{
			get => Model.Outline;
			set => Model.Outline = value;
		}
		
		public new void Dispose()
		{
            EventDispatcher.UnregisterKeyDown(this);
			UpdateManager.Remove(this);
			World.RemoveItem(this);
			base.Dispose();
		}
	}
}
