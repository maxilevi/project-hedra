using Hedra.Engine.Management;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.SkillSystem;
using OpenTK;

namespace Hedra.Engine.EntitySystem
{
    public class SkilledAnimableEntity : SkilledEntity, ISkilledAnimableEntity
    {
        private readonly Timer _timer;
        public Animation AnimationBlending { get; private set; }

        public SkilledAnimableEntity()
        {
            _timer = new Timer(1)
            {
                AutoReset = false
            };
        }
        
        public void ResetModel()
        {
        }

        public void BlendAnimation(Animation Animation)
        {
            AnimationBlending = Animation;
            _timer.AlertTime = Animation?.Length ?? 1;
            _timer.Reset();
        }

        public bool CaptureMovement { get; set; }
        
        public void Orientate()
        {
        }

        public override void Update()
        {
            base.Update();
            if (AnimationBlending == null) return;
            AnimationBlending.DispatchEvents(_timer.Progress);
            _timer.Tick();
        }

        public bool IsAttacking { get; set; }
        public bool WasAttacking { get; set; }
        public bool InAttackStance { get; set; }
        public Vector3 LookingDirection => Orientation;
    }
}