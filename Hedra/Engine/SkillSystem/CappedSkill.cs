namespace Hedra.Engine.SkillSystem
{
    public abstract class CappedSkill : BaseSkill
    {
        protected abstract int MaxLevel { get; }
        
        public override void Update()
        {
            if(Level > MaxLevel)
                Player.AbilityTree.SetPoints(GetType(), MaxLevel);
        }
    }
}