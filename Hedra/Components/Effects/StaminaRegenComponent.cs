using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;

namespace Hedra.Components.Effects
{
    public class StaminaRegenComponent : Component<IHumanoid>
    {
        private readonly float _previousValue;
            
        public StaminaRegenComponent(IHumanoid Entity, float Value) : base(Entity)
        {
            Parent.StaminaRegen += _previousValue = Value;
        }

        public override void Update(){}
        public override void Dispose()
        {
            base.Dispose();
            if(Disposed) return;
            Parent.StaminaRegen -= _previousValue;
        }
    }
}