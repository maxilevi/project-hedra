using Hedra.Engine.Management;

namespace Hedra.Engine.SkillSystem
{
    public abstract class ConditionedPassiveSkill : PassiveSkill
    {
        private readonly Timer _timer = new Timer(.15f);
        private bool _canDo;
        
        public sealed override void Update()
        {
            base.Update();
            if (Level > 0 && _timer.Tick())
            {
                _canDo = CheckIfCanDo();
                InvokeStateUpdated();
            }
        }

        protected abstract bool CheckIfCanDo();
        public override float IsAffectingModifier => _canDo ? 1 : 0;
    }
}