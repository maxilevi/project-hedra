using System.Globalization;
using Hedra.Core;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.WorldBuilding;

namespace Hedra.Engine.SkillSystem.Archer.Hunter
{
    public class SpikeTrap : SingleAnimationSkill
    {
        public override uint TextureId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/SpikeTrap.png");
        protected override int MaxLevel => 15;
        protected override bool CanMoveWhileCasting => false;
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/ArcherLayTrap.dae");

        protected override void OnAnimationEnd()
        {
            Place();
        }

        protected override void OnUse()
        {
            Player.SearchComponent<DamageComponent>().OnDamageEvent += OnCasterDamaged;
        }

        protected override void OnDisable()
        {
            Player.SearchComponent<DamageComponent>().OnDamageEvent -= OnCasterDamaged;
        }

        private void OnCasterDamaged(DamageEventArgs Args)
        {
            if(Args.Victim.IsDead) return;
            Player.KnockForSeconds(3);
            Cancel();
        }

        private void Place()
        {
            var trap = new BearTrap(Player, (Player.Model.RightWeaponPosition + Player.Model.LeftWeaponPosition) * .5f, Duration, Damage, Stun);
            World.AddWorldObject(trap);
        }
        
        private float Duration => 80 + Level * 5;
        private float Damage => 50 + Level * 4;
        private float StunChance => .10f + Level / 100f;
        private bool Stun => Utils.Rng.NextFloat() < StunChance;
        public override float ManaCost => 40;
        public override float MaxCooldown => 18;
        public override string Description => Translations.Get("spike_trap_desc");
        public override string DisplayName => Translations.Get("spike_trap");
        public override string[] Attributes => new[]
        {
            Translations.Get("spike_trap_damage_change", Damage.ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("spike_trap_duration_change", ((int)Duration).ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("spike_trap_stun_change", (int) (StunChance * 100f))
        };
    }
}