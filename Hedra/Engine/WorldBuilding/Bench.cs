using Hedra.Core;
using Hedra.Engine.Localization;
using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using OpenTK;

namespace Hedra.Engine.WorldBuilding
{
    public class Bench : InteractableStructure
    {
        protected override bool SingleUse => false;
        protected override bool CanInteract => !HasHost;
        private readonly Vector3 _sitOffset;
        private readonly Vector3 _sitRotation;
        private IHumanoid _currentHost;
        
        public Bench(Vector3 Position, Vector3 SitRotation, Vector3 SitOffset) : base(Position)
        {
            _sitOffset = SitOffset;
            _sitRotation = SitRotation;
        }

        public override void Update()
        {
            base.Update();
            if(HasHost && !_currentHost.IsSitting)
                SetHost(null);
        }

        private void SetHost(IHumanoid Host)
        {
            _currentHost = Host;
            if (HasHost)
            {
                Host.Physics.TargetPosition = Position + _sitOffset;
                Host.Rotation = _sitRotation;
            }
        }

        protected override void Interact(IHumanoid Humanoid)
        {
            if(HasHost) return;
            Humanoid.IsSitting = !Humanoid.IsSitting;
            SetHost(Humanoid.IsSitting ? Humanoid : null);
            Humanoid.CanInteract = false;
            TaskScheduler.After(500, delegate
            {
                Humanoid.CanInteract = true;
            });
        }

        private bool HasHost => _currentHost != null;
        
        public override string Message => Translations.Get("to_sit");

        public override int InteractDistance => 12;
    }
}