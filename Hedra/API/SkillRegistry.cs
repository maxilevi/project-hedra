using System;
using Hedra.Engine.SkillSystem;

namespace Hedra.API
{
    public class SkillRegistry : TypeRegistry
    {
        protected override void DoAdd(string Key, Type Value)
        {
            SkillFactory.Instance.Register(Key, Value);
        }

        protected override void DoRemove(string Key, Type Value)
        {
            SkillFactory.Instance.Unregister(Key);
        }

        protected override Type RegistryType => typeof(BaseSkill);
    }
}