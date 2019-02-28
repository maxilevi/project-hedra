using Hedra.Engine.EntitySystem;
using Hedra.EntitySystem;

namespace Hedra.Components.Effects
{
    public class AttackResistanceBonusComponent : Component<IHumanoid>
    {
        private readonly float _previousValue;
        
        public AttackResistanceBonusComponent(IHumanoid Entity, float Change) : base(Entity)
        {
            Parent.AttackResistance += _previousValue = Change;
        }

        public override void Update(){}

        public override void Dispose()
        {
            base.Dispose();
            if(Disposed) return;
            Parent.AttackResistance -= _previousValue;
        }
    }
}