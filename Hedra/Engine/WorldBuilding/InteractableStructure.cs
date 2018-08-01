﻿/*
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
using Hedra.Engine.Generation;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
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
	    public virtual float InteractionAngle => .9f;
	    public virtual Key Key => Key.E;
        public abstract string Message { get; }
        public abstract int InteractDistance { get; }
	    public bool Interacted { get; private set; }
        public event OnInteraction OnInteractEvent;
	    private bool _canInteract;
	    private bool _shouldInteract;

        protected InteractableStructure()
	    {
	        EventDispatcher.RegisterKeyDown(this, delegate (object Sender, KeyEventArgs EventArgs)
	        {
	            if (_canInteract && Key == EventArgs.Key)
	            {
	                _shouldInteract = true;
                    EventArgs.Cancel();
	            }
	        }, EventPriority.Normal);
	        UpdateManager.Add(this);
        }

	    public void Update()
	    {
	        var player = GameManager.Player;

	        bool IsInLookingAngle() => Vector2.Dot((this.Position - player.Model.Model.Position).Xz.NormalizedFast(),
                player.View.LookingDirection.Xz.NormalizedFast()) > .9f;	            
	        
            bool IsInRadius() => (this.Position - player.Position).LengthSquared < InteractDistance * InteractDistance;

	        if (IsInLookingAngle() && IsInRadius())
	        {
                player.MessageDispatcher.ShowMessageWhile($"[{Key.ToString()}] {Message}", () => !Disposed && IsInLookingAngle() && IsInRadius());
	            _canInteract = true;

	            if (_shouldInteract && !Interacted && !Disposed)
	            {
					this.InvokeInteraction(player);
                }
	            else
	            {
	                _shouldInteract = false;
                }
	        }
        }

		public void InvokeInteraction(IPlayer Player)
		{
			Interacted = true;
			this.Interact(Player);
			OnInteractEvent?.Invoke(Player);
			this.Dispose();
		}

	    public abstract void Interact(IPlayer Interactee);

	    public override void Dispose()
	    {
            base.Dispose();
	        UpdateManager.Remove(this);
	        EventDispatcher.UnregisterKeyDown(this);
        }
	}
}
