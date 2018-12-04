using Hedra.Engine.Player;
using Hedra.Engine.Sound;
using Hedra.Sound;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.WorldBuilding
{
    public class Tombstone : InteractableStructure
    {
        public override Key Key => Key.F;
        public override string Message => "TO PAY RESPECTS";
        public override int InteractDistance => 8;

        protected override void Interact(IPlayer Interactee)
        {
            SoundPlayer.PlaySound(SoundType.NotificationSound, this.Position, false, 1f, 0.6f);            
        }

        public Tombstone(Vector3 Position) : base(Position)
        {
        }
    }
}
