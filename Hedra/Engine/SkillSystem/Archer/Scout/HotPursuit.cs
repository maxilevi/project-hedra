namespace Hedra.Engine.SkillSystem.Archer.Scout
{
    public class HotPursuit : PassiveSkill
    {
        public override string Description { get; }
        public override string DisplayName { get; }
        protected override int MaxLevel { get; }
        public override uint TextureId { get; }
        protected override void Remove()
        {
            throw new System.NotImplementedException();
        }

        protected override void Add()
        {
            throw new System.NotImplementedException();
        }
    }
}