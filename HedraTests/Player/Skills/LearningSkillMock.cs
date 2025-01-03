using System;
using Hedra.Engine.ItemSystem;
using Hedra.Engine.SkillSystem;
using Hedra.Items;

namespace HedraTests.Player.Skills
{
    public class LearningSkillMock : LearningSkill
    {
        public override string Description { get; }
        public override string DisplayName { get; }
        
        public override uint IconId { get; }
        
        public Action OnLearnCallback { get; set; }

        protected override int RestrictionIndex { get; }
        protected override EquipmentType Equipment { get; }

        protected override void Add()
        {
            OnLearnCallback?.Invoke();
        }

        protected override void Remove()
        {
        }
    }
}