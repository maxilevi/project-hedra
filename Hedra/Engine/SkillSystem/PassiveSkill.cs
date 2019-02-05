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
                if(_currentLevel != 0 && Level == 0)
                    Remove();
                else
                    OnChange();
                _currentLevel = Level;
            }
        }

        protected abstract void Remove();

        protected abstract void OnChange();
        
        public override void Use()
        {
            throw new ArgumentException($"Passive skills cannot be used.");
        }
    }
}