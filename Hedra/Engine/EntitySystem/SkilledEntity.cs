using System;
using Hedra.Engine.SkillSystem;

namespace Hedra.Engine.EntitySystem
{
    public class SkilledEntity : Entity, ISkilledEntity
    {
        public float Mana
        {
            get => float.PositiveInfinity;
            set { }
        }

        public bool CanCastSkill => true;

        public void SetSkillPoints(Type Skill, int Points)
        {
        }

        public T SearchSkill<T>() where T : AbstractBaseSkill
        {
            return default;
        }
    }
}