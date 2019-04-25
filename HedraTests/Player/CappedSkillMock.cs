using Hedra.Engine.Player;
using Hedra.Engine.SkillSystem;

namespace HedraTests.Player
{
    public class CappedSkillMock : CappedSkill<IPlayer>
    {
        private int _maxLevel = 0;
        protected override int MaxLevel => _maxLevel;

        public void SetMaxLevel(int NewLevel)
        {
            _maxLevel = NewLevel;
        }

        public override string Description => throw new System.NotImplementedException();
        public override float ManaCost => throw new System.NotImplementedException();
        public override float MaxCooldown => throw new System.NotImplementedException();
        public override string DisplayName => throw new System.NotImplementedException();
        public override uint IconId => 0;

        protected override void DoUse()
        {
            throw new System.NotImplementedException();
        }
    }
}