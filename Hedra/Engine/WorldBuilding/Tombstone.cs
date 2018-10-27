using Hedra.Engine.Player;
using Hedra.Engine.Sound;
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
            SoundManager.PlaySound(SoundType.NotificationSound, this.Position, false, 1f, 0.6f);            
        }
    }
}
