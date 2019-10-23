using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using System.Numerics;

namespace Hedra.Engine.EntitySystem
{
    public class HealthBonusComponent : Component<IHumanoid>
    {
        private readonly float _healthBonus;

        public HealthBonusComponent(IHumanoid Parent, float Health) : base(Parent)
        {
            _healthBonus = Health;
            Parent.BonusHealth += _healthBonus;
        }

        public override void Update(){}

        public override void Dispose()
        {
            if(Disposed) return;
            Parent.BonusHealth -= _healthBonus;
            base.Dispose();
        }
    }
}
