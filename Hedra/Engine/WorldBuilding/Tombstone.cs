using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Sound;
using Hedra.EntitySystem;
using Hedra.Sound;
using OpenTK;
using OpenTK.Input;

namespace Hedra.Engine.WorldBuilding
{
    public class Tombstone : InteractableStructure
    {
        public override Key Key => Controls.Respect;
        public override string Message => Translations.Get("pay_respects");
        public override int InteractDistance => 8;

        protected override void Interact(IHumanoid Humanoid)
        {
            SoundPlayer.PlaySound(SoundType.NotificationSound, this.Position, false, 1f, 0.6f);            
        }

        public Tombstone(Vector3 Position) : base(Position)
        {
        }
    }
}
