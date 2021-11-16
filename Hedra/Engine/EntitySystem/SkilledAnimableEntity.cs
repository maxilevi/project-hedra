using System.Numerics;
using Hedra.Core;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.SkillSystem;

namespace Hedra.Engine.EntitySystem
{
    public class SkilledAnimableEntity : SkilledEntity, ISkilledAnimableEntity
    {
        private readonly Timer _blendTimer;
        private readonly Timer _playTimer;

        public SkilledAnimableEntity()
        {
            _blendTimer = new Timer(1)
            {
                AutoReset = false
            };
            _playTimer = new Timer(1)
            {
                AutoReset = false
            };
        }

        public Animation AnimationPlaying { get; private set; }
        public Animation AnimationBlending { get; private set; }

        public void ResetModel()
        {
        }

        public void BlendAnimation(Animation Animation)
        {
            AnimationBlending = Animation;
            _blendTimer.AlertTime = Animation?.Length ?? 1;
            _blendTimer.Reset();
        }

        public void PlayAnimation(Animation Animation)
        {
            AnimationPlaying = Animation;
            _playTimer.AlertTime = Animation?.Length ?? 1;
            _playTimer.Reset();
        }

        public bool CaptureMovement { get; set; }

        public void Orientate()
        {
        }

        public override void Update()
        {
            base.Update();
            if (AnimationBlending != null)
            {
                AnimationBlending.DispatchEvents(_blendTimer.Progress);
                _blendTimer.Tick();
            }

            if (AnimationPlaying != null)
            {
                AnimationPlaying.DispatchEvents(_playTimer.Progress);
                _playTimer.Tick();
            }
        }

        public bool IsAttacking { get; set; }
        public bool WasAttacking { get; set; }
        public bool InAttackStance { get; set; }
        public Vector3 LookingDirection => Orientation;
    }
}