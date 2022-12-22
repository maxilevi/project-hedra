using Hedra.Core;
using Hedra.Engine.Player;

namespace Hedra.Engine.SkillSystem
{
    public abstract class PlayerActivateDurationSkill : ActivateDurationSkill<IPlayer>
    {
    }

    public abstract class ActivateDurationSkill<T> : CappedSkill<T> where T : ISkillUser
    {
        private readonly Timer _timer;
        private bool _active;

        protected ActivateDurationSkill()
        {
            _timer = new Timer(1);
        }

        public override float IsAffectingModifier => _active ? 1 : 0;
        protected abstract float Duration { get; }
        protected abstract float CooldownDuration { get; }
        public sealed override float MaxCooldown => Duration + CooldownDuration;

        public sealed override void Update()
        {
            base.Update();
            if (_timer.Tick() && _active) DisableEffect();
        }

        protected override void DoUse()
        {
            EnableEffect();
        }

        private void EnableEffect()
        {
            _timer.Reset();
            _timer.AlertTime = Duration;
            _active = true;
            DoEnable();
            InvokeStateUpdated();
        }

        private void DisableEffect()
        {
            _active = false;
            DoDisable();
        }

        protected abstract void DoEnable();
        protected abstract void DoDisable();
    }
}