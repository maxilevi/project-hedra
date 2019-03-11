using Hedra.Engine.Management;

namespace Hedra.Engine.SkillSystem
{
    public abstract class DurationSingleAnimationSkillWithStance : SingleAnimationSkillWithStance
    {
        protected const float DamageInterval = 1f;
        private readonly Timer _damageTimer = new Timer(DamageInterval);
        private readonly Timer _durationTimer = new Timer(1)
        {
            AutoReset = false
        };

        protected virtual void OnDamageInterval()
        {
            
        }
        
        protected override void DoStart()
        {
            _durationTimer.AlertTime = Duration;
            _durationTimer.Reset();
            _damageTimer.Reset();
        }

        protected override void DoEnd()
        {
        }

        public override void Update()
        {
            base.Update();
            if(!IsActive) return;
            _durationTimer.Tick();
            if (_damageTimer.Tick())
            {
                OnDamageInterval();
            }
        }

        protected abstract float Duration { get; }
        protected override bool ShouldQuitStance => Player.IsMoving || _durationTimer.Ready;
    }
}