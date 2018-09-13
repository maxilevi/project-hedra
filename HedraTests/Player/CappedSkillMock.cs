﻿using Hedra.Engine.Player.Skills;

namespace HedraTests.Player
{
    public class CappedSkillMock : CappedSkill
    {
        private int _maxLevel = 0;
        protected override int MaxLevel => _maxLevel;

        public void SetMaxLevel(int NewLevel)
        {
            _maxLevel = NewLevel;
        }

        public override string Description => throw new System.NotImplementedException();
        
        public override string DisplayName => throw new System.NotImplementedException();
        
        public override void Use()
        {
            throw new System.NotImplementedException();
        }
    }
}