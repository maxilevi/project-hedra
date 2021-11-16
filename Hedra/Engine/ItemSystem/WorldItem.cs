/*
 * Created by SharpDevelop.
 * User: Maxi Levi
 * Date: 05/05/2016
 * Time: 09:31 p.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Events;
using Hedra.Engine.Player;
using Hedra.Engine.WorldBuilding;
using Hedra.Game;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;
using SixLabors.ImageSharp;

namespace Hedra.Engine.ItemSystem
{
    public delegate void OnItemCollect(IPlayer Player);

    public class WorldItem : WorldObject
    {
        private static ushort _itemCounter;
        private readonly float _height;
        private readonly bool _initialized;
        private bool _canPickup;
        private bool _disposed;
        private bool _isColliding;
        private readonly Vector3 _originalPosition;
        private bool _shouldPickup;

        public WorldItem(Item ItemSpecification, Vector3 Position) : base(null)
        {
            var modelData = ItemSpecification.Model.Clone();
            this.ItemSpecification = ItemSpecification;
            ItemId = ++_itemCounter;
            Model = ObjectMesh.FromVertexData(modelData);
            Model.OutlineColor = ItemUtils.TierToColor(ItemSpecification.Tier).ToVector4();
            Model.BaseTint = EffectDescriber.EffectColorFromItem(ItemSpecification);
            Scale = new Vector3(1.5f, 1.5f, 1.5f);
            this.Position = Position;
            _originalPosition = Position;
            _height = Math.Abs(modelData.SupportPoint(-Vector3.UnitY).Y - modelData.SupportPoint(Vector3.UnitY).Y);
            OnPickup += Player => PickedUp = true;

            EventDispatcher.RegisterKeyDown(this, delegate(object Sender, KeyEventArgs EventArgs)
            {
                if (_canPickup && Controls.Interact == EventArgs.Key) _shouldPickup = true;
            });
            _initialized = true;
        }

        public bool PickedUp { get; private set; }
        public ushort ItemId { get; }
        public Item ItemSpecification { get; }

        public bool Outline
        {
            get => Model.Outline;
            set => Model.Outline = value;
        }

        public event OnItemCollect OnPickup;

        public override void Update()
        {
            if (!_initialized) return;
            base.Update();
            Model.Alpha = Alpha;
            if (PickedUp) return;

            Position = new Vector3(_originalPosition.X,
                Math.Max(_originalPosition.Y + _height,
                    _originalPosition.Y + _height + (float)Math.Cos(Time.AccumulatedFrameTime)),
                _originalPosition.Z);
            Model.LocalRotation += Vector3.UnitY * 35f * Time.DeltaTime;

            float DotFunc()
            {
                return Vector2.Dot((Position - GameManager.Player.Position).Xz().NormalizedFast(),
                    LocalPlayer.Instance.View.LookingDirection.Xz().NormalizedFast());
            }

            if (DotFunc() > .9f && (Position - LocalPlayer.Instance.Position).LengthSquared() < 12f * 12f)
            {
                LocalPlayer.Instance.MessageDispatcher.ShowMessageWhile(
                    Translations.Get("to_pickup", Controls.Interact),
                    () => !Disposed && DotFunc() > .9f &&
                          (Position - LocalPlayer.Instance.Position).LengthSquared() < 14f * 14f);
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
                        GameManager.Player.MessageDispatcher.ShowNotification(Translations.Get("full_inventory"),
                            Color.Red, 3f, true);
                    }
                }
            }
            else
            {
                _canPickup = false;
            }

            Model.Tint = _canPickup ? Vector4.One * 1.5f : Vector4.One;
            Model.Outline = true;

            if ((Position - GameManager.Player.Position).Xz().LengthSquared() < 12 * 12 &&
                ItemSpecification.HasAttribute(CommonAttributes.Amount)
                && GameManager.Player.Inventory.Search(I => I.Name == ItemSpecification.Name) != null)
                if (!PickedUp && !Disposed)
                    OnPickup?.Invoke(GameManager.Player);
        }

        public new void Dispose()
        {
            _disposed = true;
            base.Dispose();
            EventDispatcher.UnregisterKeyDown(this);
        }
    }
}