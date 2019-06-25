using Hedra.Core;
using Hedra.Engine.Management;
using Hedra.Engine.Player;

namespace Hedra.Engine.SkillSystem
{
    public abstract class ActivateDurationSkill : ActivateDurationSkill<IPlayer>
    {
    }
    
    public abstract class ActivateDurationSkill<T> : CappedSkill<T> where T : ISkillUser
    {
        public override float IsAffectingModifier => _active ? 1 : 0;
        private readonly Timer _timer;
        private bool _active;

        protected ActivateDurationSkill()
        {
            _timer = new Timer(1);
        }
        
        public sealed override void Update()
        {
            base.Update();
            if (_timer.Tick() && _active)
            {
                Disable();
            }
        }

        protected override void DoUse()
        {
            Enable();
        }

        private void Enable()
        {
            _timer.Reset();
            _timer.AlertTime = Duration;
            _active = true;
            DoEnable();
            InvokeStateUpdated();
        }

        private void Disable()
        {
            _active = false;
            DoDisable();
        }
        
        protected abstract void DoEnable();
        protected abstract void DoDisable();        
        protected abstract float Duration { get; }
        protected abstract float CooldownDuration { get; }
        public sealed override float MaxCooldown => Duration + CooldownDuration;
    }
}