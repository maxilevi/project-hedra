using Hedra.Engine.CacheSystem;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.EntitySystem;
using Hedra.WeaponSystem;

namespace Hedra.Engine.SkillSystem.Rogue.Assassin
{
    public class MarkedForDeath : CappedSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/MarkedForDeath.png");
        private Weapon _activeWeapon;
        private bool _isActive;

        protected override void DoUse()
        {
            Player.BeforeDamaging += BeforeDamaging;
            Player.AfterDamaging += AfterDamaging;
            Player.AfterAttack += AfterAttack;
            _isActive = true;
        }

        private void BeforeDamaging(IEntity Victim, float Damage)
        {
            if (Victim.SearchComponent<MarkedForDeathComponent>() == null)
            {
                Victim.AddComponent(new MarkedForDeathComponent(Victim, AttackResistanceBonus));
            }
        }

        public override void Update()
        {
            base.Update();
            if (_isActive && _activeWeapon != Player.LeftWeapon)
            {
                if (_activeWeapon != null) _activeWeapon.Outline = false;
                _activeWeapon = Player.LeftWeapon;
                _activeWeapon.Outline = true;
                _activeWeapon.OutlineColor = Colors.Red;
            }
        }

        private void AfterDamaging(IEntity Victim, float Damage)
        {
            Player.BeforeDamaging -= BeforeDamaging;
            Player.AfterDamaging -= AfterDamaging;
            Player.AfterAttack -= AfterAttack;
            _isActive = false;
            _activeWeapon.Outline = false;
            _activeWeapon = null;
            SetOnCooldown();
        }

        private void AfterAttack(AttackOptions Options)
        {
            if(_isActive)
                AfterDamaging(default(IEntity), default(float));
        }

        private class MarkedForDeathComponent : EntityComponent
        {
            public MarkedForDeathComponent(IEntity Entity, float AttackResistanceBonus) : base(Entity)
            {
                Entity.ShowIcon(CacheItem.DeathIcon);
                Entity.AttackResistance -= AttackResistanceBonus;
            }

            public override void Update()
            {
            }
        }

        protected override bool HasCooldown => !ShouldDisable;
        public override float IsAffectingModifier => _isActive ? 1 : 0;
        protected override bool ShouldDisable => _isActive;
        private float AttackResistanceBonus => .2f + .3f * (Level / (float) MaxLevel);
        protected override int MaxLevel => 15;
        public override float ManaCost => 40;
        public override float MaxCooldown => 87;
        public override string Description => Translations.Get("marked_for_death_desc");
        public override string DisplayName => Translations.Get("marked_for_death_skill");
        public override string[] Attributes => new[]
        {
            Translations.Get("marked_for_death_resistance_change", (int)(AttackResistanceBonus * 100))
        };
    }
}