using Hedra.Components;
using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.Localization;
using Hedra.Rendering;
using Hedra.WeaponSystem;
using Hedra.WorldObjects;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Archer.Hunter
{
    public class Concealment : SingleAnimationSkillWithStance
    {
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/ArcherConcealment.dae");
        protected override Animation StanceAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/ArcherConcealmentStance.dae");
        protected override bool ShouldDisable => User.Toolbar.DisableAttack || !(User.LeftWeapon is Bow) || base.ShouldDisable;
        private Bow _bow;

        protected override void OnAnimationEnd()
        {
            if(!(User.LeftWeapon is Bow bow)) return;
            _bow = bow;
            Start();
        }
        
        protected override void DoStart()
        {
            User.Model.Outline = true;
            User.Model.OutlineColor = new Vector4(.2f, .2f, .2f, 1);
            User.LeftWeapon.SecondaryAttackEnabled = false;
            User.BeforeAttack += BeforeAttack;
            _bow.BowModifiers += AddModifiers;
            User.SearchComponent<DamageComponent>().OnDamageEvent += OnDamaged;
        }

        protected override void DoEnd()
        {
            User.Model.BaseTint = Vector4.Zero;
            User.LeftWeapon.SecondaryAttackEnabled = true;
            User.BeforeAttack -= BeforeAttack;
            User.SearchComponent<DamageComponent>().OnDamageEvent -= OnDamaged;
            _bow.BowModifiers -= AddModifiers;
            User.Model.Outline = false;
        }

        private void OnDamaged(DamageEventArgs Args)
        {
            if (!(Args.Amount > 0) || Args.Damager == null || Args.Damager == User) return;
            End();
            User.KnockForSeconds(5);
        }
        
        private static void AddModifiers(Projectile Proj)
        {
            Proj.Mesh.Outline = true;
            Proj.Mesh.OutlineColor = Colors.Violet;
            Proj.Mesh.Scale *= 1.15f;
            Proj.Speed *= 2.25f;
            Proj.UsePhysics = false;
        }
        
        private void BeforeAttack(AttackOptions Options)
        {
            Options.DamageModifier *= 1 + DamageChange;
        }

        protected override bool ShouldQuitStance => User.IsMoving || !(User.LeftWeapon is Bow);
        public override float MaxCooldown => 18;
        public override float ManaCost => 0;
        protected override int MaxLevel => 15;
        private float DamageChange => 1 + Level / 5f;
        public override string Description => Translations.Get("concealment_desc");
        public override string DisplayName => Translations.Get("concealment_skill");
        public override uint IconId { get; } = Graphics2D.LoadFromAssets("Assets/Skills/Concealment.png");
        public override string[] Attributes => new[]
        {
            Translations.Get("concealment_damage_change", (int)(DamageChange * 100))
        };
    }
}