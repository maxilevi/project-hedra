using Hedra.Engine.Management;

namespace Hedra.Engine.SkillSystem
{
    public abstract class ActivateSkill : CappedSkill
    {
        public override float IsAffectingModifier => _active ? 1 : 0;
        private readonly Timer _timer;
        private bool _active;

        protected ActivateSkill()
        {
            _timer = new Timer(1);
        }
        
        public override void Update()
        {
            base.Update();
            if (_timer.Tick() && _active)
            {
                Disable();
            }
        }
        
        public override void Use()
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
    }
}