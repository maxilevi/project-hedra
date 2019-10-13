/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/09/2016
 * Time: 12:00 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Localization;
using OpenToolkit.Mathematics;

using Silk.NET.Input.Common;


namespace Hedra.Engine.WorldBuilding
{
    /// <summary>
    /// Description of ClaimableStrucuture.
    /// </summary>
    
    public delegate void OnInteraction(IEntity Interactee);    
    
    public abstract class InteractableStructure : BaseStructure, ITickable
    {
        public int UpdatesPerSecond => 30;
        protected virtual float InteractionAngle => .75f;
        protected virtual bool SingleUse => true;
        protected virtual bool DisposeAfterUse => true;
        protected virtual bool CanInteract => true;
        protected virtual bool AllowThroughCollider => true;
        public virtual Key Key => Controls.Interact;
        public abstract string Message { get; }
        public abstract int InteractDistance { get; }
        public bool Interacted { get; private set; }
        public event OnInteraction OnInteractEvent;
        private bool _canInteract;
        private bool _shouldInteract;
        private bool _selected;

        protected InteractableStructure(Vector3 Position) : base(Position)
        {
            EventDispatcher.RegisterKeyDown(this, OnKeyDown, EventPriority.High);
            BackgroundUpdater.Add(this);
        }

        protected virtual void OnKeyDown(object Sender, KeyEventArgs EventArgs)
        {
            if (_canInteract && Key == EventArgs.Key && (!Interacted || !SingleUse) && GameManager.Player.CanInteract)
            {
                _shouldInteract = true;
                EventArgs.Cancel();
            }
        }
        
        public virtual void Update(float DeltaTime)
        {
            if ((Position - GameManager.Player.Position).LengthSquared < 128 * 128)
                DoUpdate(DeltaTime);
        }

        protected virtual void DoUpdate(float DeltaTime)
        {
            var player = GameManager.Player;

            bool IsInLookingAngle() => Vector2.Dot((this.Position - player.Position).Xz.NormalizedFast(),
                player.View.LookingDirection.Xz.NormalizedFast()) > InteractionAngle && (AllowThroughCollider || !player.Physics.Raycast(Position));                
            
            if (IsInRadius() && IsInLookingAngle() && (!Interacted || !SingleUse) && CanInteract && player.CanInteract)
            {
                player.MessageDispatcher.ShowMessageWhile($"[{Key.ToString()}] {Message}", () => !Disposed && IsInRadius() && IsInLookingAngle() && player.CanInteract);
                _canInteract = true;
                if(!_selected) OnSelected(player);
                if (_shouldInteract && (!Interacted || !SingleUse) && !Disposed && CanInteract && player.CanInteract)
                {
                    InvokeInteraction(player);
                    if (!SingleUse) _shouldInteract = false;
                }
                else
                {
                    _shouldInteract = false;
                }
            }
            else
            {
                _canInteract = false;
                if (_selected) this.OnDeselected(player);
            }
        }

        private bool IsInRadius()
        {
            return (Position - GameManager.Player.Position).LengthSquared < InteractDistance * InteractDistance;
        }

        public void InvokeInteraction(IHumanoid Humanoid)
        {
            Interacted = true;
            this.Interact(Humanoid);
            OnInteractEvent?.Invoke(Humanoid);
            if(DisposeAfterUse && SingleUse) this.Dispose();
        }

        protected virtual void OnSelected(IHumanoid Humanoid)
        {
            _selected = true;
        }

        protected virtual void OnDeselected(IHumanoid Humanoid)
        {
            _selected = false;
        }

        protected abstract void Interact(IHumanoid Humanoid);
        
        public override void Dispose()
        {
            base.Dispose();
            BackgroundUpdater.Remove(this);
            EventDispatcher.UnregisterKeyDown(this);
        }
    }
}
