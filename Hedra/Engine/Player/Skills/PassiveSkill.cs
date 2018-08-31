using System;

namespace Hedra.Engine.Player.Skills
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
            if (_currentLevel != Level) _set = false;			
            if (!_set)
            {
                _currentLevel = Level;
                if(Level > MaxLevel)
                    Player.AbilityTree.SetPoints(GetType(), MaxLevel);
                OnChange();
            }
        }

        protected abstract void OnChange();
        
        public override void Use()
        {
            throw new ArgumentException($"Passive skills cannot be used.");
        }
    }
}