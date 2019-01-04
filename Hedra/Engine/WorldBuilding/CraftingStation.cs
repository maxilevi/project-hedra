using System.Windows.Forms;
using Hedra.Core;
using Hedra.Engine.Game;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.WorldBuilding
{
    public abstract class CraftingStation : BaseStructure, IUpdatable
    {
        private const int Distance = 12;
        private const float Angle = .75f;
        
        protected abstract string CraftingMessage { get; }

        protected CraftingStation(Vector3 Position) : base(Position)
        {
            UpdateManager.Add(this);
        }

        public virtual void Update()
        {
            var player = GameManager.Player;
            bool IsInLookingAngle() => Vector2.Dot((Position - player.Position).Xz.NormalizedFast(),
                                           player.View.LookingDirection.Xz.NormalizedFast()) > Angle;

            if (IsInRadius() && IsInLookingAngle() && !player.InterfaceOpened)
            {
                player.MessageDispatcher.ShowMessageWhile($"[{Controls.Crafting.ToString()}] {CraftingMessage}",
                    () => !Disposed && IsInLookingAngle() && IsInRadius() && !player.InterfaceOpened);
            }
        }
        
        private bool IsInRadius()
        {
            return (Position - GameManager.Player.Position).LengthSquared < Distance * Distance;
        }
        
        public override void Dispose()
        {
            base.Dispose();
            UpdateManager.Remove(this);
        }
    }
}