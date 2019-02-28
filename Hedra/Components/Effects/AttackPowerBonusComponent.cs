using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;

namespace Hedra.Components.Effects
{
    public class AttackPowerBonusComponent : Component<IHumanoid>
    {
        private readonly float _previousValue;
            
        public AttackPowerBonusComponent(IHumanoid Entity, float Change) : base(Entity)
        {
            Parent.AttackPower += _previousValue = Change;
        }

        public override void Update(){}
        public override void Dispose()
        {
            base.Dispose();
            if(Disposed) return;
            Parent.AttackPower -= _previousValue;
        }
    }
}