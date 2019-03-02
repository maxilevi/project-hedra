using System;

namespace Hedra.Engine.SkillSystem
{
    public abstract class PassiveSkill : BaseSkill
    {
        public override bool Passive => true;
        protected abstract int MaxLevel { get; }
        public abstract override uint TextureId { get; }
        private bool _set;
        private int _currentLevel;

        public override void Update()
        {
            if(_currentLevel == 0 && Level == 0) return;
            
            if(_currentLevel != Level) _set = false;            
            if (!_set)
            {
                if(Level > MaxLevel)
                    Player.AbilityTree.SetPoints(GetType(), MaxLevel);
                if(_currentLevel != 0) Remove();
                Add();
                _currentLevel = Level;
                _set = true;
            }
        }

        protected abstract void Add();
        
        protected abstract void Remove();

        protected override void DoUse()
        {
            throw new ArgumentException($"Passive skills cannot be used.");
        }
    }
}