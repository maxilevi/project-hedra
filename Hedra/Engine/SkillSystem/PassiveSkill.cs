using System;
using Hedra.Engine.Player;

namespace Hedra.Engine.SkillSystem
{
    public abstract class PassiveSkill : BaseSkill<IPlayer>
    {
        private int _currentLevel;
        private bool _set;
        public override bool Passive => true;
        protected abstract int MaxLevel { get; }
        public abstract override uint IconId { get; }
        public override float ManaCost => 0;
        public override float MaxCooldown => 0;

        public override void Update()
        {
            if (_currentLevel == 0 && Level == 0) return;

            if (_currentLevel != Level) _set = false;
            if (!_set)
            {
                if (Level > MaxLevel)
                    User.AbilityTree.SetPoints(GetType(), MaxLevel);
                if (_currentLevel != 0) Remove();
                if (Level > 0) Add();
                _currentLevel = Level;
                _set = true;
            }
        }

        protected abstract void Add();

        protected abstract void Remove();

        protected override void DoUse()
        {
            throw new ArgumentException("Passive skills cannot be used.");
        }
    }
}