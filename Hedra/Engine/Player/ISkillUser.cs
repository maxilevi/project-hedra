using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.SkillSystem;

namespace Hedra.Engine.Player
{
    public interface ISkillUser : IEntityWithAttributes, IObjectWithMana
    {
        bool CanCastSkill { get; }
        void SetSkillPoints(Type Skill, int Points);
        T SearchSkill<T>() where T : AbstractBaseSkill;
    }
}