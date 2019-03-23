using Hedra.Engine.Management;
using Hedra.AISystem;
using Hedra.AISystem.Behaviours;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;

namespace Hedra.Components.Effects
{
    public class FearComponent : EntityComponent
    {
        private BasicAIComponent _previousAi;
        private FearAIComponent _fearComponent;
        private readonly Timer _duration;
        
        public FearComponent(IEntity Entity, IEntity Attacker, float Duration, float Slowness) : base(Entity)
        {
            Parent.AddComponent(new SlowingComponent(Parent, Attacker, Duration, Slowness));               
            Parent.ShowIcon(CacheItem.FearIcon, Duration);
            Parent.RemoveComponent(_previousAi = Parent.SearchComponent<BasicAIComponent>(), false);
            Parent.AddComponent(_fearComponent = new FearAIComponent(Parent, Attacker));
            _duration = new Timer(Duration);
        }

        public override void Update()
        {
            if (_duration.Tick())
                Kill();
        }

        private void Kill()
        {
            Executer.ExecuteOnMainThread(() => Parent.RemoveComponent(this));
        }
        
        public override void Dispose()
        {
            Parent.RemoveComponent(_fearComponent);
            Parent.AddComponent(_previousAi);
        }
    }
}