using System;
using System.Linq;
using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;

namespace Hedra.AISystem.Behaviours
{
    public abstract class HunterHostileBehaviour : HostileBehaviour
    {
        protected HunterHostileBehaviour(IEntity Parent) : base(Parent)
        {
        }

        protected override IEntity GetTarget()
        {
            return GetTargets<IEntity>().FirstOrDefault(E => Array.IndexOf(HuntTypes, E.MobType) != -1);
        }
        
        protected abstract MobType[] HuntTypes { get; }
    }
}