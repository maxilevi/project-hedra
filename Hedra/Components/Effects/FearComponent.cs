using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;

namespace Hedra.Components.Effects
{
    public class FearComponent : EntityComponent
    {
        public FearComponent(IEntity Entity, IEntity Attacker, float Duration, float Slowness) : base(Entity)
        {
            Parent.AddComponent(new SlowingComponent(Parent, Attacker, Duration, Slowness));               
            Parent.ShowIcon(CacheItem.FearIcon, Duration);
        }

        public override void Update()
        {
            throw new System.NotImplementedException();
        }
    }
}