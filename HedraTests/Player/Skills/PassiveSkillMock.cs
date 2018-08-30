using System;
using Hedra.Engine.Player.Skills;

namespace HedraTests.Player.Skills
{
    public class PassiveSkillMock : PassiveSkill
    {
        private int _maxLevel;
        public override string Description => string.Empty;
        protected override int MaxLevel => _maxLevel;
        public Action OnChangeCallback { get; set; }
             
        protected override void OnChange()
        {
            OnChangeCallback?.Invoke();
        }

        public void SetMaxLevel(int Value)
        {
            _maxLevel = Value;
        }
    }
}