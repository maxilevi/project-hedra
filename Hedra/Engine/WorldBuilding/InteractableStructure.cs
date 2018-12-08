/*
 * Created by SharpDevelop.
 * User: maxi
 * Date: 19/09/2016
 * Time: 12:00 a.m.
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.WorldBuilding
{
    /// <summary>
    /// Description of ClaimableStrucuture.
    /// </summary>
    
    public delegate void OnInteraction(IEntity Interactee);    
    
    public abstract class InteractableStructure : BaseStructure, IUpdatable
    {
        protected virtual float InteractionAngle => .75f;
        protected virtual bool SingleUse => true;
        protected virtual bool DisposeAfterUse => true;
        protected virtual bool CanInteract => true;
        public virtual Key Key => Key.E;
        public abstract string Message { get; }
        public abstract int InteractDistance { get; }
        public bool Interacted { get; private set; }
        public event OnInteraction OnInteractEvent;
        private bool _canInteract;
        private bool _shouldInteract;
        private bool _selected;

        protected InteractableStructure(Vector3 Position) : base(Position)
        {
            EventDispatcher.RegisterKeyDown(this, delegate (object Sender, KeyEventArgs EventArgs)
            {
                if (_canInteract && Key == EventArgs.Key && (!Interacted || !SingleUse))
                {
                    _shouldInteract = true;
                    EventArgs.Cancel();
                }
            }, EventPriority.Normal);
            UpdateManager.Add(this);
        }

        public virtual void Update()
        {
            if ((Position - GameManager.Player.Position).LengthSquared < 128 * 128)
                DoUpdate();
        }

        protected virtual void DoUpdate()
        {
            var player = GameManager.Player;

            bool IsInLookingAngle() => Vector2.Dot((this.Position - player.Position).Xz.NormalizedFast(),
                player.View.LookingDirection.Xz.NormalizedFast()) > InteractionAngle;                
            
            if (IsInRadius() && IsInLookingAngle() && (!Interacted || !SingleUse) && CanInteract)
            {
                player.MessageDispatcher.ShowMessageWhile($"[{Key.ToString()}] {Message}", () => !Disposed && IsInLookingAngle() && IsInRadius());
                _canInteract = true;
                if(!_selected) OnSelected(player);
                if (_shouldInteract && (!Interacted || !SingleUse) && !Disposed && CanInteract)
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

        public void InvokeInteraction(IPlayer Player)
        {
            Interacted = true;
            this.Interact(Player);
            OnInteractEvent?.Invoke(Player);
            if(DisposeAfterUse && SingleUse) this.Dispose();
        }

        protected virtual void OnSelected(IPlayer Interactee)
        {
            _selected = true;
        }

        protected virtual void OnDeselected(IPlayer Interactee)
        {
            _selected = false;
        }

        protected abstract void Interact(IPlayer Interactee);

        public override void Dispose()
        {
            base.Dispose();
            UpdateManager.Remove(this);
            EventDispatcher.UnregisterKeyDown(this);
        }
    }
}
