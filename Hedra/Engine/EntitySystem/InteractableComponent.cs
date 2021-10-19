
using Hedra.Core;
using Hedra.Engine.Events;
using Hedra.Engine.Game;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using Hedra.Game;
using Hedra.Localization;
using System.Numerics;
using Hedra.Numerics;
using Silk.NET.Input;

namespace Hedra.Engine.EntitySystem
{
    public abstract class InteractableComponent : EntityComponent
    {
        public virtual float InteractionAngle => .9f;
        public virtual Key Key => Controls.Interact;
        public abstract string Message { get; }
        public abstract int InteractDistance { get; }
        protected bool Interacted { get; private set; }
        private bool _canInteract;
        private bool _shouldInteract;

        protected InteractableComponent(IEntity Entity) : base(Entity)
        {
            EventDispatcher.RegisterKeyDown(this, delegate (object Sender, KeyEventArgs EventArgs)
            {
                if (_canInteract && Key == EventArgs.Key)
                {
                    _shouldInteract = true;
                }
            });
        }

        public override void Update()
        {
            var player = GameManager.Player;

            bool IsInLookingAngle() => Vector2.Dot((Parent.Position - player.Position).Xz().NormalizedFast(),
                                           player.View.LookingDirection.Xz().NormalizedFast()) > .9f;
            bool IsInRadius() => (Parent.Position - player.Position).LengthSquared() < InteractDistance * InteractDistance;

            if (IsInLookingAngle() && IsInRadius())
            {
                player.MessageDispatcher.ShowMessageWhile($"[{Key.ToString()}] {Message}",
                    () => !Disposed && IsInLookingAngle() && IsInRadius());
                Parent.Model.Tint = new Vector4(1.5f, 1.5f, 1.5f, 1.0f);
                _canInteract = true;

                if (_shouldInteract && !Interacted && !Disposed)
                {
                    this.Interact(player);
                }
                else
                {
                    _shouldInteract = false;
                }
            }
            else
            {
                Parent.Model.Tint = Vector4.One;
            }
        }

        public virtual void Interact(IPlayer Interactee)
        {
            Interacted = true;
        }

        public override void Dispose()
        {
            base.Dispose();
            EventDispatcher.UnregisterKeyDown(this);
        }
    }
}
