namespace Hedra.Engine.Player.Skills
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