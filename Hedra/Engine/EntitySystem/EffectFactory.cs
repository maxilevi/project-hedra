using System;
using Hedra.Engine.Core;
using Hedra.Engine.ModuleSystem.Templates;
using Hedra.EntitySystem;

namespace Hedra.Engine.EntitySystem
{
    public class EffectFactory : RegistryFactory<EffectFactory, string, Type>
    {
        public EntityComponent Build(EffectTemplate Template, IEntity Mob)
        {
            var type = this[Template.Name];
            if (typeof(ApplyEffectComponent).IsAssignableFrom(type))
                return ApplyEffectBuilder(Template, Mob);
            return DefaultBuilder(Template, Mob);
        }

        private ApplyEffectComponent ApplyEffectBuilder(EffectTemplate Template, IEntity Mob)
        {
            return (ApplyEffectComponent)Activator.CreateInstance(
                this[Template.Name],
                Mob,
                (int)Template.Chance,
                Template.Damage,
                Template.Duration
            );
        }

        private EntityComponent DefaultBuilder(EffectTemplate Template, IEntity Mob)
        {
            return (EntityComponent)Activator.CreateInstance(
                this[Template.Name],
                Mob
            );
        }
    }
}