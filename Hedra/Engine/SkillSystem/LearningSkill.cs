using Hedra.Items;

namespace Hedra.Engine.SkillSystem
{
    public abstract class LearningSkill : PassiveSkill
    {
        public sealed override bool Passive => true;
        protected sealed override int MaxLevel => 1;
        protected abstract int RestrictionIndex { get; }
        protected abstract EquipmentType Equipment { get; }

        protected override void Add()
        {
            User.Inventory.AddRestriction(RestrictionIndex, Equipment);
        }

        protected override void Remove()
        {
            User.Inventory.RemoveRestriction(RestrictionIndex, Equipment);
        }
    }
}