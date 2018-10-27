using System;
using Hedra.Engine.Player.Skills;

namespace HedraTests.Player.Skills
{
    public class PassiveSkillMock : PassiveSkill
    {
        private int _maxLevel;
        public override string Description => string.Empty;
        public override string DisplayName => string.Empty;
        protected override int MaxLevel => _maxLevel;
        public override uint TextureId => 0;
        public Action OnChangeCallback { get; set; }
        public Action OnRemoveCallback { get; set; }
             
        protected override void OnChange()
        {
            OnChangeCallback?.Invoke();
        }

        protected override void Remove()
        {
            OnRemoveCallback?.Invoke();
        }

        public void SetMaxLevel(int Value)
        {
            _maxLevel = Value;
        }
    }
}