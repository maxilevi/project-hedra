using System.Globalization;
using BulletSharp;
using Hedra.Components;
using Hedra.Engine.Bullet;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering.Animation;
using Hedra.Engine.WorldBuilding;
using Hedra.Localization;
using Hedra.Numerics;
using Hedra.Rendering;

namespace Hedra.Engine.SkillSystem.Archer.Hunter
{
    public class SpikeTrap : SingleAnimationSkill<IPlayer>
    {
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/SpikeTrap.png");
        protected override int MaxLevel => 15;
        protected override bool CanMoveWhileCasting => false;

        protected override Animation SkillAnimation { get; } =
            AnimationLoader.LoadAnimation("Assets/Chr/ArcherLayTrap.dae");

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
            Translations.Get("spike_trap_duration_change",
                ((int)Duration).ToString("0.0", CultureInfo.InvariantCulture)),
            Translations.Get("spike_trap_stun_change", (int)(StunChance * 100f))
        };

        protected override void OnAnimationEnd()
        {
            Place();
        }

        protected override void OnEnable()
        {
            User.SearchComponent<DamageComponent>().OnDamageEvent += OnCasterDamaged;
        }

        protected override void OnDisable()
        {
            User.SearchComponent<DamageComponent>().OnDamageEvent -= OnCasterDamaged;
        }

        private void OnCasterDamaged(DamageEventArgs Args)
        {
            if (Args.Victim.IsDead) return;
            User.KnockForSeconds(3);
            Cancel();
        }

        private void Place()
        {
            var position = (User.Model.RightWeaponPosition + User.Model.LeftWeaponPosition) * .5f;
            var callback =
                BulletPhysics.Raycast(position.Compatible(), position.Xz().ToVector3().Compatible(),
                    BulletPhysics.TerrainFilter | CollisionFilterGroups.StaticFilter);
            var trap = new BearTrap(User, callback.HitPointWorld.Compatible(), Duration, Damage, Stun);
            trap.Ignore(X => X == User.Companion.Entity);
        }
    }
}