namespace Hedra.Engine.SkillSystem
{
    public abstract class LearningSkill : PassiveSkill
    {
        public override bool Passive => true;
        protected override int MaxLevel => 1;

        protected override void OnChange()
        {
            Learn();
        }

        protected override void Remove()
        {
            
        }

        protected abstract void Learn();
    }
}
