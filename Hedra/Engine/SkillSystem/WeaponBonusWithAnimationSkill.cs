using System;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Player;
using Hedra.EntitySystem;
using System.Numerics;

namespace Hedra.Engine.SkillSystem
{
    public abstract class WeaponBonusWithAnimationSkill : SingleAnimationSkill<IPlayer>
    {
        private readonly WeaponBonusSkillComposition _skill;
        
        protected WeaponBonusWithAnimationSkill()
        {
            _skill = new WeaponBonusSkillComposition
            {
                ApplyBonusToEnemyPublic = ApplyBonusToEnemy,
                OutlineColorPublic = OutlineColor
            };
        }

        protected override void DoUse()
        {
            base.DoUse();
            _skill.DoUsePublic();
        }

        public override void Update()
        {
            base.Update();
            _skill.Update();
        }

        protected abstract void ApplyBonusToEnemy(IEntity Victim, ref float Damage);
        protected abstract Vector4 OutlineColor { get; }
        
        public override float IsAffectingModifier => _skill.IsAffectingModifier;
        protected override bool HasCooldown => _skill.HasCooldownPublic;
        protected override bool ShouldDisable => _skill.ShouldDisablePublic;

        private class WeaponBonusSkillComposition : WeaponBonusSkill
        {
            public override string Description => throw new NotImplementedException();
            public override string DisplayName => throw new NotImplementedException();
            public override uint IconId => throw new NotImplementedException();
            public override float ManaCost => throw new NotImplementedException();
            public override float MaxCooldown => 0;

            protected override void ApplyBonusToEnemy(IEntity Victim, ref float Damage) => ApplyBonusToEnemyPublic(Victim, ref Damage);
            protected override Vector4 OutlineColor => OutlineColorPublic;
            protected override int MaxLevel => 99;

            public void DoUsePublic() => DoUse();
            public OnDamageModifierEventHandler ApplyBonusToEnemyPublic { get; set; }
            public Vector4 OutlineColorPublic { get; set; }
            public bool ShouldDisablePublic => ShouldDisable;
            public bool HasCooldownPublic => HasCooldown;
        }
    }
}