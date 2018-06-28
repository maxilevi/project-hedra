using Hedra.Engine.Generation;
using Hedra.Engine.Player;
using OpenTK;

namespace Hedra.Engine.EntitySystem
{
    internal class HealthBonusComponent : EntityComponent
    {
        private readonly float _healthBonus;
        public new Humanoid Parent;

        public HealthBonusComponent(Humanoid Parent, float Health) : base(Parent)
        {
            this.Parent = Parent;
            _healthBonus = Health;
            Parent.AddonHealth += _healthBonus;
        }

        public override void Update() {}

        public override void Dispose()
        {
            Parent.AddonHealth -= _healthBonus;
        }
    }
}
