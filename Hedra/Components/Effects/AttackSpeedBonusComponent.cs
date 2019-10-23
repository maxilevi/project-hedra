using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.Rendering;
using System.Numerics;

namespace Hedra.Components.Effects
{
    public class AttackSpeedBonusComponent : Component<IHumanoid>
    {
        private readonly float _attackSpeedBonus;
        private readonly bool _showParticles; 

        public AttackSpeedBonusComponent(IHumanoid Parent, float AttackSpeed, bool ShowParticles = false) : base(Parent)
        {
            _showParticles = ShowParticles;
            _attackSpeedBonus = AttackSpeed;
            Parent.AttackSpeed = Parent.BaseAttackSpeed + _attackSpeedBonus;
        }

        public override void Update()
        {
            if(Parent is Humanoid human && human.IsRiding || !Parent.IsMoving || !_showParticles) return;
            if (_attackSpeedBonus > 0)
            {
                Parent.Model.Outline = true;
                Parent.Model.OutlineColor = Colors.Yellow;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public override void Dispose()
        {
            if(Disposed) return;
            Parent.AttackSpeed = Parent.BaseAttackSpeed  - _attackSpeedBonus;
            if(_showParticles)
                Parent.Model.Outline = false;
        }
    }
}
