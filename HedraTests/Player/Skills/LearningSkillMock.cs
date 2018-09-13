using System;
using Hedra.Engine.Player.Skills;

namespace HedraTests.Player.Skills
{
    public class LearningSkillMock : LearningSkill
    {
        public override string Description { get; }
        public override string DisplayName { get; }
        
        public override uint TextureId { get; protected set; }
        
        public Action OnLearnCallback { get; set; }
        
        protected override void Learn()
        {
            OnLearnCallback?.Invoke();
        }
    }
}