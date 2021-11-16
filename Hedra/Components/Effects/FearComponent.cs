using Hedra.AISystem;
using Hedra.Core;
using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Management;
using Hedra.EntitySystem;

namespace Hedra.Components.Effects
{
    public class FearComponent : EntityComponent
    {
        private readonly Timer _duration;
        private readonly FearAIComponent _fearComponent;
        private readonly BasicAIComponent _previousAi;

        public FearComponent(IEntity Entity, IEntity Attacker, float Duration, float Slowness) : base(Entity)
        {
            Parent.AddComponent(new SlowingComponent(Parent, Attacker, Duration, Slowness));
            Parent.ShowIcon(CacheItem.FearIcon, Duration);
            _previousAi = Parent.SearchComponent<BasicAIComponent>();
            if (_previousAi != null)
            {
                Parent.RemoveComponent(_previousAi, false);
                Parent.AddComponent(_fearComponent = new FearAIComponent(Parent, Attacker));
            }

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
            if (_fearComponent != null) Parent.RemoveComponent(_fearComponent);
            if (_previousAi != null) Parent.AddComponent(_previousAi);
        }
    }
}