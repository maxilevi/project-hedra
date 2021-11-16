using System.Numerics;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Game;
using Hedra.Localization;
using Hedra.Numerics;

namespace Hedra.Engine.WorldBuilding
{
    public abstract class CraftingStation : BaseStructure, IUpdatable
    {
        private const int Distance = 12;
        private const float Angle = .75f;

        protected CraftingStation(Vector3 Position) : base(Position)
        {
            BackgroundUpdater.Add(this);
        }

        protected abstract string CraftingMessage { get; }

        public abstract Crafting.CraftingStation StationType { get; }

        public virtual bool CanCraft => true;

        public virtual void Update()
        {
            if (!CanCraft) return;
            var player = GameManager.Player;

            bool IsInLookingAngle()
            {
                return Vector2.Dot((Position - player.Position).Xz().NormalizedFast(),
                    player.View.LookingDirection.Xz().NormalizedFast()) > Angle;
            }

            if (IsInRadius() && IsInLookingAngle() && !player.InterfaceOpened)
                player.MessageDispatcher.ShowMessageWhile($"[{Controls.Crafting.ToString()}] {CraftingMessage}",
                    () => !Disposed && IsInLookingAngle() && IsInRadius() && !player.InterfaceOpened);
        }

        private bool IsInRadius()
        {
            return (Position - GameManager.Player.Position).LengthSquared() < Distance * Distance;
        }

        public override void Dispose()
        {
            base.Dispose();
            BackgroundUpdater.Remove(this);
        }
    }
}