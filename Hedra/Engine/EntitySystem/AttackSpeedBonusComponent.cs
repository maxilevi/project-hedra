using Hedra.Engine.Player;

namespace Hedra.Engine.EntitySystem
{
    public class AttackSpeedBonusComponent : EntityComponent
    {
        public new Humanoid Parent;
        private readonly float _attackSpeedBonus;

        public AttackSpeedBonusComponent(Humanoid Parent, float AttackSpeed) : base(Parent)
        {
            this.Parent = Parent;
            _attackSpeedBonus = AttackSpeed;
            Parent.AttackSpeed = Parent.BaseAttackSpeed * _attackSpeedBonus;
        }

        public override void Update()
        {
        }

        public override void Dispose()
        {
            Parent.AttackSpeed = Parent.BaseAttackSpeed / _attackSpeedBonus;
        }
    }
}
