using Hedra.Engine.EntitySystem;
using Hedra.Engine.Localization;
using Hedra.Engine.Player;
using Hedra.Engine.Rendering;
using Hedra.Engine.Rendering.Animation;
using Hedra.WeaponSystem;
using OpenTK;

namespace Hedra.Engine.SkillSystem.Archer.Hunter
{
    public class Concealment : SingleAnimationSkillWithStance
    {
        protected override Animation SkillAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/ArcherConcealment.dae");
        protected override Animation StanceAnimation { get; } = AnimationLoader.LoadAnimation("Assets/Chr/ArcherConcealmentStance.dae");
        protected override bool ShouldDisable => Player.Toolbar.DisableAttack || !(Player.LeftWeapon is Bow) || base.ShouldDisable;
        private Bow _bow;

        protected override void OnAnimationEnd()
        {
            if(!(Player.LeftWeapon is Bow bow)) return;
            _bow = bow;
            Start();
        }
        
        protected override void DoStart()
        {
            Player.Model.Outline = true;
            Player.Model.OutlineColor = new Vector4(.2f, .2f, .2f, 1);
            Player.LeftWeapon.SecondaryAttackEnabled = false;
            Player.BeforeAttack += BeforeAttack;
            _bow.BowModifiers += AddModifiers;
            Player.SearchComponent<DamageComponent>().OnDamageEvent += OnDamaged;
        }

        protected override void DoEnd()
        {
            Player.Model.BaseTint = Vector4.Zero;
            Player.LeftWeapon.SecondaryAttackEnabled = true;
            Player.BeforeAttack -= BeforeAttack;
            Player.SearchComponent<DamageComponent>().OnDamageEvent -= OnDamaged;
            _bow.BowModifiers -= AddModifiers;
            Player.Model.Outline = false;
        }

        private void OnDamaged(DamageEventArgs Args)
        {
            if (!(Args.Amount > 0) || Args.Damager == null || Args.Damager == Player) return;
            End();
            Player.KnockForSeconds(5);
        }
        
        private static void AddModifiers(Projectile Proj)
        {
            Proj.Mesh.Outline = true;
            Proj.Mesh.OutlineColor = Colors.Violet;
            Proj.Mesh.Scale *= 1.15f;
            Proj.Speed *= 1.25f;
            Proj.Falloff = 0f;
            Proj.UsePhysics = false;
        }
        
        private void BeforeAttack(AttackOptions Options)
        {
            Options.DamageModifier *= 1 + DamageChange;
        }

        protected override bool ShouldQuitStance => Player.IsMoving || !(Player.LeftWeapon is Bow);
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