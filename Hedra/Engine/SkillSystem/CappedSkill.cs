using Hedra.Engine.Player;

namespace Hedra.Engine.SkillSystem
{
    public abstract class CappedSkill<T> : BaseSkill<T> where T : ISkillUser
    {
        protected abstract int MaxLevel { get; }

        public override void Update()
        {
            if (Level > MaxLevel)
                User.SetSkillPoints(GetType(), MaxLevel);
        }
    }
}