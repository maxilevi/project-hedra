using System;
using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;

namespace Hedra.API
{
    public class EffectRegistry : TypeRegistry
    {
        protected override void DoAdd(string Key, Type Value)
        {
            EffectFactory.Instance.Register(Key, Value);
        }

        protected override void DoRemove(string Key, Type Value)
        {
            EffectFactory.Instance.Register(Key, Value);
        }

        protected override Type RegistryType => typeof(ApplyEffectComponent);
    }
}