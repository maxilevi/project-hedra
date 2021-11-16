using Hedra.Core;

namespace Hedra.Engine.SkillSystem
{
    public abstract class ConditionedPassiveSkill : PassiveSkill
    {
        private readonly Timer _timer = new Timer(.15f);
        private bool _isActive;
        public override float IsAffectingModifier => _isActive ? 1 : 0;

        protected override void Add()
        {
            _isActive = true;
            DoAdd();
        }

        protected override void Remove()
        {
            _isActive = false;
            DoRemove();
        }

        public sealed override void Update()
        {
            base.Update();
            if (Level > 0 && _timer.Tick())
            {
                var canDo = CheckIfCanDo();
                if (!canDo && _isActive) Remove();
                else if (canDo && !_isActive) Add();
                InvokeStateUpdated();
            }
        }

        protected abstract void DoAdd();
        protected abstract void DoRemove();

        protected abstract bool CheckIfCanDo();
    }
}