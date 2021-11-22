using System.Numerics;
using Hedra.Core;
using Hedra.EntitySystem;
using Hedra.Localization;

namespace Hedra.Engine.WorldBuilding
{
    public class Bench : InteractableStructure
    {
        private readonly Vector3 _sitOffset;
        private readonly Vector3 _sitRotation;
        private IHumanoid _currentHost;

        public Bench(Vector3 Position, Vector3 SitRotation, Vector3 SitOffset) : base(Position)
        {
            _sitOffset = SitOffset;
            _sitRotation = SitRotation;
        }

        protected override bool SingleUse => false;
        protected override bool CanInteract => !IsOccupied;

        public bool IsOccupied => _currentHost != null;

        public override string Message => Translations.Get("to_sit");

        public override int InteractDistance => 12;

        public override void Update(float DeltaTime)
        {
            base.Update(DeltaTime);
            if (IsOccupied && !_currentHost.IsSitting)
                SetHost(null);
        }

        private void SetHost(IHumanoid Host)
        {
            _currentHost = Host;
            if (IsOccupied)
            {
                Host.Position = Position + _sitOffset;
                Host.Rotation = _sitRotation;
                TaskScheduler.When(() => (Host.Model.ModelPosition - Host.Position).LengthSquared() < 1, () => Host.IsSitting = true);
            }
        }

        protected override void Interact(IHumanoid Humanoid)
        {
            if (IsOccupied) return;
            Humanoid.IsSitting = !Humanoid.IsSitting;
            SetHost(Humanoid.IsSitting ? Humanoid : null);
            Humanoid.CanInteract = false;
            Humanoid.Physics.UsePhysics = false;
            TaskScheduler.When(() => !Humanoid.IsSitting, () => Humanoid.Physics.UsePhysics = true);
            TaskScheduler.After(.5f, delegate { Humanoid.CanInteract = true; });
        }
    }
}