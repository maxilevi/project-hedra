using System.Numerics;
using Hedra.EntitySystem;
using Hedra.Localization;
using Hedra.Sound;
using Silk.NET.Input;

namespace Hedra.Engine.WorldBuilding
{
    public class Tombstone : InteractableStructure
    {
        public Tombstone(Vector3 Position) : base(Position)
        {
        }

        public override Key Key => Controls.Respect;
        public override string Message => Translations.Get("interact_tombstone");
        public override int InteractDistance => 8;

        protected override void Interact(IHumanoid Humanoid)
        {
            SoundPlayer.PlaySound(SoundType.NotificationSound, Position, false, 1f, 0.6f);
        }
    }
}