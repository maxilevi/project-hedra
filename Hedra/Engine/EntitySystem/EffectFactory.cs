using System;
using Hedra.Engine.Core;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.EntitySystem;

namespace Hedra.Engine.EntitySystem
{
    public class EffectFactory : RegistryFactory<EffectFactory, string, Type>
    {
        public ApplyEffectComponent Build(EffectTemplate Template, IEntity Mob)
        {
            return (ApplyEffectComponent) Activator.CreateInstance(
                this[Template.Name],
                Mob,
                (int) Template.Chance,
                Template.Damage,
                Template.Duration
            );
        }
    }
}